using Alchemist.Import.Products.Interfaces;
using Import.Interfaces;
using Import.Service;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Alchemist.Import.Products.Service;

public abstract class ShopImportCategoryProductsService<TCategory, TProductItem>(ILogger logger,
    ILoaderService loader,
    string url,
    IEnumerable<IProductShopCategory> shopCategories,
    IProductItemHandler itemHandler,
    string productPathFormat,
    string categoryPathFormat,
    string sourceName,
    ICategoryPaging<TCategory> categoryPaging,
    IItemSerializer<TCategory> categorySerializer,
    ImportProductServiceOptions importProductServiceOptions) : ImportService(logger, loader, url), IListener<IProductShopCategory>
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    public enum AttemptResult
    {
        AttemptsReachedTheMaximum,
        AttemptsAvailable,
        Success
    }

    protected sealed record ImportProduct(IProductItem ProductItem, string SourceName, string SourcePath, Stream Stream) : IImportProduct
    {
    }

    private readonly IProductItemHandler _itemHandler = itemHandler;

    private readonly SemaphoreSlim _categoryListenerSemaphoreSlim = new(1, 1);

    protected LinkedList<IProductShopCategory> Categories { get; } = new LinkedList<IProductShopCategory>(shopCategories);

    private readonly string _productPathFormat = productPathFormat;

    private readonly string _categoryPathFormat = categoryPathFormat;

    private readonly string _sourceName = sourceName;

    private readonly ICategoryPaging<TCategory> _categoryPaging = categoryPaging;

    private readonly IItemSerializer<TCategory> _categorySerializer = categorySerializer;

    protected virtual int MaxUnsuccessRequestCount => 10;

    protected virtual int ConcurrentCategoryTaskCount => 10;

    protected ImportProductServiceOptions ImportProductServiceOptions { get; } = importProductServiceOptions;

    async Task IListener<IProductShopCategory>.On(IProductShopCategory message, CancellationToken cancellationToken)
    {
        var acquired = false;
        try
        {
            await _categoryListenerSemaphoreSlim.WaitAsync(cancellationToken);
            acquired = true;

            Categories.AddLast(message);
            Logger.LogInformation(ImportProductLogMessages.NewCategoryIsEnqueued, message.Path);
        }
        finally
        {
            if (acquired)
                _categoryListenerSemaphoreSlim.Release();
        }
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        await ProcessCategories(stoppingToken);
    }

    protected virtual async Task ProcessCategories(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (Categories.Count == 0)
            {
                await Task.Delay(50, stoppingToken);
                continue;
            }

            var concurrentCategories = new List<IProductShopCategory>(ConcurrentCategoryTaskCount);

            while (Categories.Count > 0 && concurrentCategories.Count < ConcurrentCategoryTaskCount)
            {
                if (Categories.First != null)
                {
                    var productShopCategory = Categories.First?.Value;
                    concurrentCategories.Add(productShopCategory);
                }

                Categories.RemoveFirst();
            }

            try
            {
                await Task.WhenAll(concurrentCategories.Select(c => ProcessCategoryAsync(c, stoppingToken)));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
        }
    }

    protected virtual Task ProcessCategoryAsync(IProductShopCategory category, CancellationToken stoppingToken)
    {
        var categoryState = CategoryState.Start(category.Path);

        return ProcessCategoryAsync(category, categoryState, stoppingToken);
    }

    protected async Task ProcessCategoryAsync(IProductShopCategory category, CategoryState categoryState, CancellationToken stoppingToken)
    {
        bool? isEndOfCategory = null;

        if(string.IsNullOrEmpty(categoryState.CategoryPagePath))
            categoryState.CategoryPagePath = _categoryPaging.GetCategoryPagePath(category, 
                _categoryPathFormat,
                ImportProductServiceOptions.CategoryPathFormatType, 
                categoryState.Page);

        while (isEndOfCategory != true)
        {
            var (success, categoryResult, successCount) = await TryProcessCategoryPageAsync(categoryState.CategoryPagePath,
                category.ItemId,
                category.Path, 
                categoryState.Page, 
                stoppingToken);

            var attemptResult = ApplyCategoryStateAttempts(category, categoryState, success);
            
            if (attemptResult == AttemptResult.AttemptsReachedTheMaximum)
                break;            
            
            if (attemptResult == AttemptResult.AttemptsAvailable)            
                continue;

            if (successCount == null)
            {
                categoryState.CategoryPagePath = _categoryPaging.GetCategoryPagePath(category, _categoryPathFormat, 
                    ImportProductServiceOptions.CategoryPathFormatType, 
                    categoryState.Page, 
                    categoryResult);
                continue;
            }

            var nextCategoryPagePath = _categoryPaging.GetNextCategoryPagePath(category, _categoryPathFormat, 
                ImportProductServiceOptions.CategoryPathFormatType,
                categoryState.Page,
                categoryResult);
            categoryState.CategoryPagePath = nextCategoryPagePath;

            categoryState.ApplyPageIterationResult(successCount, categoryResult?.CategoryProductItems.Length);

            isEndOfCategory = categoryResult != null && (IsLastPage(categoryResult, categoryState.Page) == true
                || IsEndOfCategory(categoryResult, categoryState.AllProductCount) == true);

            categoryState.IncrementPage();
        }

        Logger.LogInformation(ImportProductLogMessages.CategoryCompletedInfo, category.Category, categoryState.SuccessProductCount, categoryState.UnsuccessProductCount);
    }

    protected AttemptResult ApplyCategoryStateAttempts(IProductShopCategory category, CategoryState categoryState, bool success)
    {
        if (!success)
        {
            categoryState.IncrementAttempts();

            if (categoryState.AttemptsCount == MaxUnsuccessRequestCount)
            {
                Logger.LogError(ImportProductLogMessages.FailedToLoadCategoryPageAfterAttempts, category.Path, categoryState.Page, MaxUnsuccessRequestCount);
                return AttemptResult.AttemptsReachedTheMaximum;
            }

            return AttemptResult.AttemptsAvailable;
        }
        else
            categoryState.ResetAttempts();

        return AttemptResult.Success;
    }

    protected bool? IsLastPage(TCategory category, int page)
    {
        var totalCount = category.TotalCount;
        if (totalCount > 0 && ImportProductServiceOptions.PageProductCount > 0)
        {
            var totalPageCount = totalCount % ImportProductServiceOptions.PageProductCount > 0
                ? totalCount / ImportProductServiceOptions.PageProductCount + 1
                : totalCount / ImportProductServiceOptions.PageProductCount;

            return page >= totalPageCount;
        }

        return null;
    }

    protected async Task<(bool success, TCategory?, int? successCount)> TryProcessCategoryPageAsync(string categoryPagePath, int categoryItemId, string categoryPath, int page, CancellationToken stoppingToken)
    {
        var loaderData = GetLoaderData();
        var (success, category) = await TryGetCategoryFromPathAsync(categoryPagePath,
            ImportProductServiceOptions.CategoryLoadData != null
                ? new object?[] { loaderData, ImportProductServiceOptions.CategoryLoadData, new string[] { $"{categoryItemId}", $"{page}" } }
                : loaderData,
            categoryPath,
            page,
            stoppingToken);

        if (!success)
        {
            Logger.LogInformation(ImportProductLogMessages.CategoryNotLoadedFromUrl, categoryPagePath);
            return (false, default, default);
        }

        if (!(category?.CategoryProductItems?.Length > 0))
            return (true, category, default);
        int successCount = await ProcessCategoryProductItems(categoryItemId, category, stoppingToken);

        if (successCount == category.CategoryProductItems.Length)
            Logger.LogInformation(ImportProductLogMessages.CategoryProductsForUrlWereProcessedSuccesfullInfo, categoryPagePath, successCount);
        else if (successCount == 0)
            Logger.LogInformation(ImportProductLogMessages.CategoryProductsWereNotLoadedError, categoryPagePath);
        else if (successCount < category.CategoryProductItems.Length)
            Logger.LogInformation(ImportProductLogMessages.NotAllCategoryProductsForUrlWereProcessedWarning, categoryPagePath);

        return (true, category, successCount);
    }

    protected virtual async Task<int> ProcessCategoryProductItems(int categoryItemId, TCategory category, CancellationToken stoppingToken)
    {
        var successCount = 0;
        foreach (var categoryProductItem in category.CategoryProductItems)
        {
            categoryProductItem.CategoryItemId = categoryItemId;

            if (await TryProcessCategoryProductAsync(categoryProductItem, stoppingToken))
                successCount++;
        }

        return successCount;
    }

    protected virtual async Task<bool> TryProcessCategoryProductAsync(ICategoryProductItem categoryProductItem, CancellationToken cancellationToken)
    {
        var path = GetProductAbsolutePath(_productPathFormat, categoryProductItem);

        var loaderData = GetLoaderData();
        (bool success, Stream? stream) = await TryLoadProductItemAsync(categoryProductItem, path, loaderData, cancellationToken);

        if (!success || stream is null)
        {
            if (stream is not null)
                await stream.DisposeAsync();

            Logger.LogInformation(ImportProductLogMessages.ProductWasNotLoadedFromUrlWithError, path);
            return false;
        }

        var productItem = CreateProductItemFromCategoryProductItem(categoryProductItem, path);

        Logger.LogInformation(ImportProductLogMessages.ProductHasBeenSuccessfullyLoadedFromUrl, productItem.Name, path);

        await TryHandleProduct(productItem, stream, cancellationToken);
        await stream.DisposeAsync();

        return true;
    }

    protected async Task<(bool success, Stream? stream)> TryLoadProductItemAsync(ICategoryProductItem categoryProductItem, string path, object? loaderData, CancellationToken cancellationToken)
    {
        bool success;
        Stream? stream;
        try
        {
            (success, stream) = await TryLoadFromUrlAsync(path,
                ImportProductServiceOptions.ProductLoadData is not null
                    ? new object?[] { loaderData, ImportProductServiceOptions.ProductLoadData, new string[] { $"{categoryProductItem.Id}" } }
                    : loaderData,
                cancellationToken);
        }
        catch
        {
            Logger.LogInformation(ImportProductLogMessages.FailedToLoadProductOfCategory, path, categoryProductItem.CategoryItemId);
            throw;
        }

        return (success, stream);
    }

    protected async Task<bool> TryHandleProduct(TProductItem productItem, Stream stream, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _itemHandler.HandleItem(new ImportProduct(productItem, _sourceName, Host, stream), cancellationToken);
            Logger.LogInformation(ImportProductLogMessages.ProductFromUrlHandledWithStatusInfo, productItem.Name, productItem.AbsolutePath, result);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, ImportProductLogMessages.ProductFromUrlHandlingFailed, productItem.AbsolutePath);
            return false;
        }
    }

    protected virtual async Task<(bool success, TCategory? result)> TryGetCategoryFromPathAsync(string dataPath, object? requestData, string categoryPath, int page, CancellationToken token)
    {
        bool success;
        Stream? stream;
        try
        {
            (success, stream) = await TryLoadFromUrlAsync(dataPath, requestData, cancellationToken: token);
        }
        catch
        {
            Logger.LogInformation(ImportProductLogMessages.FailedToLoadCategoryPage, categoryPath, page);
            throw;
        }

        if (!success || stream is null)
        {
            if (stream is not null)
                await stream.DisposeAsync();

            return (false, default(TCategory?));
        }

        await using var categoryStream = stream;

        Logger.LogInformation(ImportProductLogMessages.CategoryPageLoadedSuccessfully, categoryPath, page);

        try
        {
            var item = await _categorySerializer.DeserializeFromStream(stream, cancellationToken: token); 
            return (true, item);            
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, ImportProductLogMessages.SerializationFailed, typeof(TCategory).Name, dataPath);
            return (false, default(TCategory?));
        }  
        finally
        {
            await stream.DisposeAsync();
        }
    }

    protected TProductItem CreateProductItemFromCategoryProductItem(ICategoryProductItem categoryProductItem, string productPath)
    {  
        var productItem = new TProductItem
        {
            Path = categoryProductItem.ItemPath,
            ItemId = categoryProductItem.Id,
            Name = categoryProductItem.Name,
            Price = categoryProductItem.Price,
            Currency = categoryProductItem.Currency,
            AbsolutePath = productPath,
            CategoryId = categoryProductItem.CategoryItemId,
            Brand = categoryProductItem.Brand
        };

        return productItem;
    }

    protected virtual bool? IsEndOfCategory(TCategory category, int processProductCount)
    {
        return category.TotalCount <= processProductCount;
    }

    protected virtual string GetProductAbsolutePath(string productPathFormat, ICategoryProductItem productItem)
    {
        return ImportProductServiceOptions.ProductPathFormatType switch
        {
            PathFormatType.ItemId => string.Format(productPathFormat, productItem.Id),
            PathFormatType.Path => string.Format(productPathFormat, PreparePath(productItem.ItemPath)),
            PathFormatType.CategoryWithItemId => string.Format(productPathFormat, productItem.Id, PreparePath(productItem.ItemPath)),
            _ => string.Format(productPathFormat, productItem.ItemPath),
        };
    }

    protected abstract string PreparePath(string path);

    protected override void Dispose()
    {
        _categoryListenerSemaphoreSlim.Dispose();

        base.Dispose();
    }
}