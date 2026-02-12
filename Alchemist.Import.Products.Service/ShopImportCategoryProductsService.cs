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
    ImportProductServiceOptions importProductServiceOptions) : ImportService(logger, loader, url), IListener<IProductShopCategory>
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    public record CountResult(int SuccessCount, int UnsuccessCount);

    protected sealed record ImportProduct(IProductItem ProductItem, string SourceName, string SourcePath, Stream Stream) : IImportProduct
    {
    }

    private readonly IProductItemHandler _itemHandler = itemHandler;

    private readonly SemaphoreSlim _categoryListenerSemaphoreSlim = new(1, 1);

    protected ConcurrentQueue<IProductShopCategory> Categories { get; } = new();

    private readonly IEnumerable<IProductShopCategory> _initialCategories = shopCategories;

    private readonly string _productPathFormat = productPathFormat;    

    private readonly string _categoryPathFormat = categoryPathFormat; 

    private readonly string _sourceName = sourceName;    

    protected virtual int MaxUnsuccessRequestCount => 10;

    protected ImportProductServiceOptions ImportProductServiceOptions { get; } = importProductServiceOptions;

    async Task IListener<IProductShopCategory>.On(IProductShopCategory message, CancellationToken cancellationToken)
    {
        try
        {
            await _categoryListenerSemaphoreSlim.WaitAsync(cancellationToken);
            Categories.Enqueue(message);
            Logger.LogInformation(ImportProductLogMessages.NewCategoryIsEnqueued, message.Path);
        }
        finally
        {
            _categoryListenerSemaphoreSlim.Release();
        }
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(_initialCategories.Select(c => ProcessCategoryAsync(c, stoppingToken)));

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!Categories.TryDequeue(out var category))
            {
                await Task.Delay(50, stoppingToken);
                continue;
            } 

            await ProcessCategoryAsync(category, stoppingToken);
        }
    }

    private async Task ProcessCategoryAsync(IProductShopCategory category, CancellationToken stoppingToken)
    {
        int successProductCount = 0;
        int unsuccessProductCount = 0;
        int page = 1;
        bool? isEndOfCategory = null;
        var attemptsCount = 0;

        var categoryPagePath = GetCategoryPagePath(category, _categoryPathFormat, ImportProductServiceOptions.CategoryPathFormatType, page);

        while (isEndOfCategory != true)
        {
            var (success, categoryResult, categoryPageResult) = await TryProcessCategoryPageAsync(categoryPagePath, category.ItemId, category.Path, page, stoppingToken);

            if (!success)
            {
                attemptsCount++;

                if (attemptsCount == MaxUnsuccessRequestCount)
                    break;

                continue;
            }
            else
                attemptsCount = 0;

            if (categoryPageResult == null)
            {
                categoryPagePath = GetCategoryPagePath(category, _categoryPathFormat, ImportProductServiceOptions.CategoryPathFormatType, page, categoryResult);
                continue;
            }

            successProductCount += categoryPageResult?.SuccessCount ?? 0;
            unsuccessProductCount += categoryPageResult?.UnsuccessCount ?? 0;

            isEndOfCategory = categoryResult != null && (IsLastPage(categoryResult, page) == true
                || IsEndOfCategory(categoryResult, successProductCount + unsuccessProductCount) == true);

            var nextCategoryPagePath = GetNextCategoryPagePath(category, _categoryPathFormat, ImportProductServiceOptions.CategoryPathFormatType, page, categoryResult);
            categoryPagePath = nextCategoryPagePath;
            page++;
        }

        Logger.LogInformation(ImportProductLogMessages.CategoryCompletedInfo, category.Category, successProductCount, unsuccessProductCount);
    }

    protected virtual object[] GetCategoryPathArguments(IProductShopCategory productShopCategory, PathFormatType pathFormatType)
    {
        return pathFormatType switch
        {
            PathFormatType.ItemId => [productShopCategory.ItemId],
            PathFormatType.Path => [PreparePath(productShopCategory.Path)],
            PathFormatType.CategoryWithItemId => [PreparePath(productShopCategory.Category), productShopCategory.ItemId],
            _ => [productShopCategory.Path],
        };
    }

    protected virtual string GetCategoryPagePath(IProductShopCategory productShopCategory, string pathFormat, PathFormatType pathFormatType, int page, TCategory? category = null)
    {
        List<object> args = [.. GetCategoryPathArguments(productShopCategory, pathFormatType), page];

        return string.Format(pathFormat, args: [.. args]);
    }

    protected abstract string PreparePath(string path);

    protected virtual string GetNextCategoryPagePath(IProductShopCategory productShopCategory, string pathFormat, PathFormatType pathFormatType, int page, TCategory? category = null)
    {
        return GetCategoryPagePath(productShopCategory,pathFormat, pathFormatType, page + 1, category);
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

    protected virtual async Task<(bool success, TCategory?, CountResult? result)> TryProcessCategoryPageAsync(string categoryPagePath, int categoryItemId, string categoryPath, int page, CancellationToken stoppingToken)
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
            return (false, default(TCategory), default(CountResult));
        }

        if(!(category?.CategoryProductItems?.Length > 0))
            return (true, category, default);

        var successCount = 0;
        foreach (var categoryProductItem in category.CategoryProductItems)
        {
            categoryProductItem.CategoryItemId = categoryItemId;

            if (await TryProcessCategoryProductAsync(categoryProductItem, stoppingToken))
                successCount++;
        }

        if (successCount == category.CategoryProductItems.Length)
            Logger.LogInformation(ImportProductLogMessages.CategoryProductsForUrlWereProcessedSuccesfullInfo, categoryPagePath, successCount);
        else if (successCount == 0)
            Logger.LogInformation(ImportProductLogMessages.CategoryProductsWereNotLoadedError, categoryPagePath);
        else if (successCount < category.CategoryProductItems.Length)
            Logger.LogInformation(ImportProductLogMessages.NotAllCategoryProductsForUrlWereProcessedWarning, categoryPagePath);

        return (true, category, new CountResult(successCount, category.CategoryProductItems.Length - successCount));
    }

    protected async Task<bool> TryProcessCategoryProductAsync(ICategoryProductItem categoryProductItem, CancellationToken cancellationToken)
    {
        var path = GetProductAbsolutePath(_productPathFormat, categoryProductItem);

        var loaderData = GetLoaderData();
        var (success, stream) = await TryLoadFromUrlAsync(path,
            ImportProductServiceOptions.ProductLoadData is not null
                ? new object?[] { loaderData, ImportProductServiceOptions.ProductLoadData, new string[] { $"{categoryProductItem.Id}" } }
                : loaderData,
            cancellationToken);
        
        if(!success)
        {
            Logger.LogInformation(ImportProductLogMessages.ProductWasNotLoadedFromUrlWithError, path);
            return false;
        }  

        var productItem = CreateProductItemFromCategoryItem(categoryProductItem, path);        

        Logger.LogInformation(ImportProductLogMessages.ProductHasBeenSuccessfullyLoadedFromUrl, productItem.Name, path);

        try
        {
            var result = await _itemHandler.HandleItem(new ImportProduct(productItem, _sourceName, Host, stream), cancellationToken);
            Logger.LogInformation(ImportProductLogMessages.ProductFromUrlHandledWithStatusInfo, productItem.Name, productItem.AbsolutePath, result);
        }
        catch(Exception e)
        {
            Logger.LogError(e, ImportProductLogMessages.ProductFromUrlHandlingFailed, productItem.AbsolutePath);
        }

        return true;
    }

    protected virtual async Task<(bool success, TCategory? result)> TryGetCategoryFromPathAsync(string dataPath, object? requestData, string categoryPath, int page, CancellationToken token)
    {        
        var (success, stream) = await TryLoadFromUrlAsync(dataPath, requestData, cancellationToken : token);

        if (!success || stream is null) return (false, default(TCategory?));

        Logger.LogInformation(ImportProductLogMessages.CaterogyPageLoadedSuccessfully, categoryPath, page);

        try
        {
            await using (stream)
            {
                var item = await DeserializeCategoryFromStream(stream, cancellationToken: token); 
                return (true, item);
            }
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, ImportProductLogMessages.SerializationFailed, typeof(TCategory).Name, dataPath);
            return (false, default(TCategory?));
        }   
    }

    protected virtual async Task<TCategory?> DeserializeCategoryFromStream(Stream stream, CancellationToken cancellationToken = default)
    {
        return await JsonSerializer.DeserializeAsync<TCategory>(stream, cancellationToken: cancellationToken);
    }

    protected TProductItem CreateProductItemFromCategoryItem(ICategoryProductItem categoryProductItem, string productPath)
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

    protected override void Dispose()
    {
        _categoryListenerSemaphoreSlim.Dispose();

        base.Dispose();
    }
}