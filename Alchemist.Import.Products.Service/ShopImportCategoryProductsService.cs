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
    protected sealed record ImportProduct(IProductItem ProductItem, string SourceName, string SourceUrl, Stream Stream) : IImportProduct
    {
    }

    private readonly IProductItemHandler _itemHandler;

    private readonly SemaphoreSlim _categoriyListenerSemaphoreSlim = new(1, 1);

    protected ConcurrentQueue<IProductShopCategory> Categories { get; }

    private readonly string _productUrlFormat;    

    private readonly string _categoryUrlFormat; 

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
        string productUrlFormat,
        string categoryUrlFormat,
        string sourceName,
        ImportProductServiceOptions importProductServiceOptions)
        : base(logger, loader, url)
    {
        ImportProductServiceOptions = importProductServiceOptions;            

        _productUrlFormat = productUrlFormat;
        _categoryUrlFormat = categoryUrlFormat;
        
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
            Logger.LogInformation(ImportProductLogMessages.NewCategoryIsEnqueued, [message.GetCategoryUrlWithId()]);
        }
        finally
        {
            _categoriyListenerSemaphoreSlim.Release();
        }
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        while (!Categories.IsEmpty && !stoppingToken.IsCancellationRequested)
        {
            if (!Categories.TryDequeue(out var category))
                continue;

            int successProductCount = 0;
            int unsuccessProductCount = 0;
            int page = 1;
            bool? isEndOfCategory = null;
            var attemptsCount = 0;

            var categoryPageUrl = GetCategoryPageUrl(category, _categoryUrlFormat, ImportProductServiceOptions.CategoryUrlFormatType, page);

            while (isEndOfCategory != true)
            {
                var (success, categoryResult, categoryPageResult) = await TryProcessCategoryPageAsync(categoryPageUrl, category.ItemId, page, stoppingToken);

                if(!success)
                {
                    attemptsCount++;

                    if (attemptsCount == MaxUnsuccessRequestCount)
                        break;

                    continue;
                }
                else
                    attemptsCount = 0;

                
                successProductCount += categoryPageResult.SuccessCount;
                unsuccessProductCount += categoryPageResult.UnsuccessCount;

                isEndOfCategory = IsLastPage(categoryResult, page) == true
                    || IsEndOfCategory(categoryResult, successProductCount+ unsuccessProductCount) == true;

                var nextCategoryPageUrl = GetNextCategoryPageUrl(category, _categoryUrlFormat, ImportProductServiceOptions.CategoryUrlFormatType, page + 1, categoryResult);
                categoryPageUrl = nextCategoryPageUrl;
                page++;
            }

            Logger.LogInformation(ImportProductLogMessages.CategoryCompletedInfo, [category.Category, successProductCount, unsuccessProductCount]);
        }
    }

    private static string PrepareItemUrl(string itemUrl)
    {
        itemUrl =  itemUrl.StartsWith("/") ? itemUrl[1..] : itemUrl;
        return itemUrl.EndsWith("/") ? itemUrl[..^1] : itemUrl;
    }

    protected virtual string GetCategoryPageUrl(IProductShopCategory productShopCategory, string urlFormat, UrlFormatType urlFormatType, int page, TCategory? category = null)
    {
        var args = new List<object>();

        switch (urlFormatType)
        {
            case UrlFormatType.ItemId:
                args = [productShopCategory.ItemId]; 
                break;

            case UrlFormatType.Url:
                args = [PrepareItemUrl(productShopCategory.Category)];
                break;

            case UrlFormatType.UrlWithItemId:
                args = [PrepareItemUrl(productShopCategory.Category), productShopCategory.ItemId];
                break;
        }

        args.Add(page);

        return string.Format(urlFormat, args: [.. args]);
    }

    protected virtual string GetNextCategoryPageUrl(IProductShopCategory productShopCategory, string urlFormat, UrlFormatType urlFormatType, int page, TCategory? category = null)
    {
        return GetCategoryPageUrl(productShopCategory,urlFormat, urlFormatType, page + 1, category);
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

    protected virtual async Task<(bool success, TCategory?, CountResult? result)> TryProcessCategoryPageAsync(string categoryPageUrl, int categoryItemId, int page, CancellationToken stoppingToken)
    {
        var (success, category) = await TryGetCategoryFromApiUrlAsync<TCategory>(categoryPageUrl,
            ImportProductServiceOptions.CategoryLoadData != null 
                ? new object?[] { LoadData, ImportProductServiceOptions.CategoryLoadData, new string[] { $"{categoryItemId}", $"{page}" } } 
                : LoadData,
            stoppingToken);

        if (!success)
        {
            Logger.LogInformation(ImportProductLogMessages.CategoryNotLoadedFromUrl, categoryPageUrl);
            return (false, default(TCategory), default(CountResult));
        }

        var successCount = 0;
        foreach (var categoryProductItem in category.CategoryProductItems)
        {
            categoryProductItem.CategoryItemId = categoryItemId;

            if (await TryProcessCategoryProductAsync(categoryProductItem, stoppingToken))
                successCount++;
        }

        if (successCount == category.CategoryProductItems.Length)
            Logger.LogInformation(ImportProductLogMessages.CategoryProductsForUrlWereProcessedSuccesfullInfo, [categoryPageUrl, successCount]);
        else if (successCount == 0)
            Logger.LogInformation(ImportProductLogMessages.CategoryProductsWereNotLoadedError, [categoryPageUrl]);
        else if (successCount < category.CategoryProductItems.Length)
            Logger.LogInformation(ImportProductLogMessages.NotAllCategoryProductsForUrlWereProcessedWarning, [categoryPageUrl]);

        return (true, category, new CountResult(successCount, category.CategoryProductItems.Length - successCount));
    }

    protected async Task<bool> TryProcessCategoryProductAsync(ICategoryProductItem categoryProductItem, CancellationToken cancellationToken)
    {
        var apiUrl = GetApiUrl(_productUrlFormat, categoryProductItem);

        var (success, stream) = await TryLoadFromUrlAsync(apiUrl,
            ImportProductServiceOptions.ProductLoadData is not null
                ? new object?[] { LoadData, ImportProductServiceOptions.ProductLoadData, new string[] { $"{categoryProductItem.Id}" } }
                : LoadData,
            cancellationToken);
        
        if(!success)
        {
            Logger.LogInformation(ImportProductLogMessages.ProductWasNotLoadedFromUrlWithError, apiUrl);
            return false;
        }        
        
        if (!success || stream == default) return false;

        var productItem = GetProductItemFromCategoryItem(categoryProductItem, apiUrl);        

        Logger.LogInformation(ImportProductLogMessages.ProductHasBeenSuccessfullyLoadedFromUrl, productItem.Name, apiUrl);

        try
        {
            var result = await _itemHandler.HandleItem(new ImportProduct(productItem, _sourceName, Host, stream), cancellationToken);
            Logger.LogInformation(ImportProductLogMessages.ProductFromUrlHandledWithStatusInfo, [productItem.Name, productItem.ApiUrl, result]);
        }
        catch(Exception e)
        {
            Logger.LogError(e, ImportProductLogMessages.ProductFromUrlHandlingFailed, [productItem.ApiUrl, e.Message]);
        }

        return true;
    }

    protected virtual async Task<(bool success, TCategory? result)> TryGetCategoryFromApiUrlAsync<T>(string apiUrl, object? requestData, CancellationToken token)
        where T:class
    {        
        var (success, stream) = await TryLoadFromUrlAsync(apiUrl, requestData, cancellationToken : token);

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
            Logger.LogError(ex, ImportProductLogMessages.SerializationError, [typeof(TCategory).Name, apiUrl, ex.Message]);
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

    protected TProductItem GetProductItemFromCategoryItem(ICategoryProductItem categoryProductItem, string apiUrl)
    {  
        var productItem = new TProductItem
        {
            Path = categoryProductItem.ItemUrl,
            ItemId = categoryProductItem.Id,
            Name = categoryProductItem.Name,
            Price = categoryProductItem.Price,
            Currency = categoryProductItem.Currency,
            ApiUrl = apiUrl,
            CategoryId = categoryProductItem.CategoryItemId,
            Brand = categoryProductItem.Brand
        };

        return productItem;
    }

    protected virtual bool? IsEndOfCategory(TCategory category, int processProductCount)
    {
        return category.TotalCount >= processProductCount;
    }

    protected virtual string GetApiUrl(string productUrlFormat, ICategoryProductItem productItem)
    {
        switch (ImportProductServiceOptions.ProductUrlFormatType)
        {
            case UrlFormatType.Url:
                return string.Format(productUrlFormat, PrepareItemUrl(productItem.ItemUrl));

            case UrlFormatType.UrlWithItemId:
                return string.Format(productUrlFormat, productItem.Id, PrepareItemUrl(productItem.ItemUrl));

            default:
                return string.Format(productUrlFormat, productItem.Id);
        }
    }
}