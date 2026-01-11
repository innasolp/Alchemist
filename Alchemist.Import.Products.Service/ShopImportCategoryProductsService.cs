using Alchemist.Import.Products.Interfaces;
using Import.Interfaces;
using Import.Service;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Alchemist.Import.Products.Service;

public abstract class ShopImportCategoryProductsService<TCategory, TProductItem> : ImportService, IListener<IProductShopCategory>
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    protected sealed record ImportProduct(IProductItem ProductItem, string SourceName, string SourcePath, Stream Stream) : IImportProduct
    {
    }

    private readonly IProductItemHandler _itemHandler;

    private readonly SemaphoreSlim _categoriyListenerSemaphoreSlim = new(1, 1);

    protected ConcurrentQueue<IProductShopCategory> Categories { get; }

    private readonly string _productPathFormat;    

    private readonly string _categoryPathFormat; 

    private readonly string _sourceName;    

    protected virtual int MaxUnsuccessRequestCount => 10;

    protected ImportProductServiceOptions ImportProductServiceOptions { get; }

    public object GetData(object loadData, string httpMethod, string dataFormat, params object[] parameters)
    {
        if (dataFormat is null) return new object[2] { httpMethod, loadData };

        if (dataFormat.StartsWith("{")) dataFormat = $"{{{dataFormat}}}";

        return new object[3] { httpMethod, loadData, string.Format(dataFormat, parameters) };
    }
   
    public ShopImportCategoryProductsService(ILogger logger,
        ILoaderService loader,
        string url,
        IEnumerable<IProductShopCategory> shopCategories,
        IProductItemHandler itemHandler,
        string productPathFormat,
        string categoryPathFormat,
        string sourceName,
        ImportProductServiceOptions importProductServiceOptions)
        : base(logger, loader, url)
    {
        ImportProductServiceOptions = importProductServiceOptions;            

        _productPathFormat = productPathFormat;
        _categoryPathFormat = categoryPathFormat;
        
        _sourceName = sourceName;
        _itemHandler = itemHandler;
        Categories = new();

        shopCategories.ToList().ForEach(c => Categories.Enqueue(c));               
    }

    async Task IListener<IProductShopCategory>.On(IProductShopCategory message)
    {
        try
        {
            await _categoriyListenerSemaphoreSlim.WaitAsync();
            Categories.Enqueue(message);
            Logger.LogInformation(ImportProductLogMessages.NewCategoryIsEnqueued, [message.Path]);
        }
        finally
        {
            _categoriyListenerSemaphoreSlim.Release();
        }
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!Categories.TryDequeue(out var category))            
                continue;            

            int successProductCount = 0;
            int unsuccessProductCount = 0;
            int page = 1;
            bool? isEndOfCategory = null;
            var attemptsCount = 0;

            var categoryPagePath = GetCategoryPagePath(category, _categoryPathFormat, ImportProductServiceOptions.CategoryPathFormatType, page);

            while (isEndOfCategory != true)
            {
                var (success, categoryResult, categoryPageResult) = await TryProcessCategoryPageAsync(categoryPagePath, category.ItemId, page, stoppingToken);

                if(!success)
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
                    || IsEndOfCategory(categoryResult, successProductCount+ unsuccessProductCount) == true);

                var nextCategoryPagePath = GetNextCategoryPagePath(category, _categoryPathFormat, ImportProductServiceOptions.CategoryPathFormatType, page, categoryResult);
                categoryPagePath = nextCategoryPagePath;
                page++;
            }

            Logger.LogInformation(ImportProductLogMessages.CategoryCompletedInfo, [category.Category, successProductCount, unsuccessProductCount]);
        }
    }

    protected virtual string GetCategoryPagePath(IProductShopCategory productShopCategory, string pathFormat, PathFormatType pathFormatType, int page, TCategory? category = null)
    {
        var args = new List<object>();

        switch (pathFormatType)
        {
            case PathFormatType.None:
                args = [productShopCategory.Path];
                break;

            case PathFormatType.ItemId:
                args = [productShopCategory.ItemId]; 
                break;

            case PathFormatType.Path:
                args = [PreparePath(productShopCategory.Path)];
                break;

            case PathFormatType.PathWithItemId:
                args = [PreparePath(productShopCategory.Category), productShopCategory.ItemId];
                break;
        }

        args.Add(page);

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

    protected virtual async Task<(bool success, TCategory?, CountResult? result)> TryProcessCategoryPageAsync(string categoryPagePath, int categoryItemId, int page, CancellationToken stoppingToken)
    {
        var (success, category) = await TryGetCategoryFromPathAsync<TCategory>(categoryPagePath,
            ImportProductServiceOptions.CategoryLoadData != null 
                ? new object?[] { LoadData, ImportProductServiceOptions.CategoryLoadData, new string[] { $"{categoryItemId}", $"{page}" } } 
                : LoadData,
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
            Logger.LogInformation(ImportProductLogMessages.CategoryProductsForUrlWereProcessedSuccesfullInfo, [categoryPagePath, successCount]);
        else if (successCount == 0)
            Logger.LogInformation(ImportProductLogMessages.CategoryProductsWereNotLoadedError, [categoryPagePath]);
        else if (successCount < category.CategoryProductItems.Length)
            Logger.LogInformation(ImportProductLogMessages.NotAllCategoryProductsForUrlWereProcessedWarning, [categoryPagePath]);

        return (true, category, new CountResult(successCount, category.CategoryProductItems.Length - successCount));
    }

    protected async Task<bool> TryProcessCategoryProductAsync(ICategoryProductItem categoryProductItem, CancellationToken cancellationToken)
    {
        var path = GetProductAbsolutePath(_productPathFormat, categoryProductItem);

        var (success, stream) = await TryLoadFromUrlAsync(path,
            ImportProductServiceOptions.ProductLoadData is not null
                ? new object?[] { LoadData, ImportProductServiceOptions.ProductLoadData, new string[] { $"{categoryProductItem.Id}" } }
                : LoadData,
            cancellationToken);
        
        if(!success)
        {
            Logger.LogInformation(ImportProductLogMessages.ProductWasNotLoadedFromUrlWithError, path);
            return false;
        }        
        
        if (!success || stream == default) return false;

        var productItem = CreateProductItemFromCategoryItem(categoryProductItem, path);        

        Logger.LogInformation(ImportProductLogMessages.ProductHasBeenSuccessfullyLoadedFromUrl, productItem.Name, path);

        try
        {
            var result = await _itemHandler.HandleItem(new ImportProduct(productItem, _sourceName, Host, stream), cancellationToken);
            Logger.LogInformation(ImportProductLogMessages.ProductFromUrlHandledWithStatusInfo, [productItem.Name, productItem.AbsolutePath, result]);
        }
        catch(Exception e)
        {
            Logger.LogError(e, ImportProductLogMessages.ProductFromUrlHandlingFailed, [productItem.AbsolutePath, e.Message]);
        }

        return true;
    }

    protected virtual async Task<(bool success, TCategory? result)> TryGetCategoryFromPathAsync<T>(string path, object? requestData, CancellationToken token)
        where T:class
    {        
        var (success, stream) = await TryLoadFromUrlAsync(path, requestData, cancellationToken : token);

        if (!success || stream is null) return (false, default(TCategory?));

        try
        {
            using (stream)
            {
                var item = await DeserializeCategoryFromStream<T>(stream, cancellationToken: token);        
                stream.Close();
                return (true, item);
            }
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, ImportProductLogMessages.SerializationError, [typeof(TCategory).Name, path, ex.Message]);
            return (false, default(TCategory?));
        }
#if DEBUG
        catch(Exception e)
        {
            throw;
        }
#endif         
    }

    protected virtual async Task<TCategory?> DeserializeCategoryFromStream<T>(Stream stream, CancellationToken cancellationToken = default)
        where T:class
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
        switch (ImportProductServiceOptions.ProductPathFormatType)
        {
            case PathFormatType.ItemId:
                return string.Format(productPathFormat, productItem.Id);

            case PathFormatType.Path:
                return string.Format(productPathFormat, PreparePath(productItem.ItemPath));

            case PathFormatType.PathWithItemId:
                return string.Format(productPathFormat, productItem.Id, PreparePath(productItem.ItemPath));

            default:
                return string.Format(productPathFormat, productItem.ItemPath);                
        }
    }
}