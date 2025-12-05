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
    protected sealed record ImportProduct(IProductItem ProductItem, string SourceName, string SourceUrl) : IImportProduct
    {
    }

    private readonly IProductItemHandler _itemHandler;

    private readonly SemaphoreSlim _categoriyListenerSemaphoreSlim = new(1, 1);

    protected ConcurrentQueue<IProductShopCategory> Categories { get; }

    private readonly string _productUrlFormat;

    protected string CategoryUrlFormat { get; }

    private readonly string _sourceName;
    protected abstract int PageProductCount { get; }
    protected virtual int MaxUnsuccessRequestCount => 10;

    public ShopImportCategoryProductsService(ILogger logger,
        string productUrlFormat,
        string categoryUrlFormat,
        string sourceName,
        string url,
        IEnumerable<IProductShopCategory> shopCategories,
        ILoaderService loader,
        IProductItemHandler itemHandler)
        : base(logger, loader, url)
    {
        _productUrlFormat = productUrlFormat;
        CategoryUrlFormat = categoryUrlFormat;
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
            Logger.LogInformation(ImportProductLogMessages.NewCategoryIsEnqueued, [message.GetCategoryUrl()]);
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

            var categoryUrl = category.GetCategoryUrl();
            var categoryPageUrl = GetCategoryPageUrl(CategoryUrlFormat, categoryUrl, page);

            while (isEndOfCategory != true)
            {
                var (success, categoryResult, categoryPageResult) = await TryProcessCategoryPageAsync(categoryPageUrl, category.ItemId, stoppingToken);

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

                isEndOfCategory = IsLastPage(categoryResult, page) == true || IsEndOfCategory(categoryResult);

                var nextCategoryPageUrl = GetNextCategoryPageUrl(CategoryUrlFormat, categoryUrl, page + 1, categoryResult);
                categoryPageUrl = nextCategoryPageUrl;
                page++;
            }

            Logger.LogInformation(ImportProductLogMessages.CategoryCompletedInfo, [categoryUrl, successProductCount, unsuccessProductCount]);
        }
    }

    protected virtual string GetCategoryPageUrl(string urlFormat, string itemId, int page, TCategory? category = null)
    {
        return string.Format(urlFormat, itemId, page);
    }

    protected virtual string GetNextCategoryPageUrl(string urlFormat, string itemId, int page, TCategory? category = null)
    {
        return string.Format(urlFormat, itemId, page + 1);
    }

    protected bool? IsLastPage(TCategory category, int page)
    {
        var totalCount = category.TotalCount;
        if (totalCount > 0)
        {
            var totalPageCount = totalCount % PageProductCount > 0
                ? totalCount / PageProductCount + 1
                : totalCount / PageProductCount;

            return page >= totalPageCount;
        }

        return null;
    }

    protected virtual async Task<(bool success, TCategory?, CountResult? result)> TryProcessCategoryPageAsync(string categoryPageUrl, int categoryItemId, CancellationToken stoppingToken)
    {
        var (success, category) = await TryGetFromApiUrlAsync<TCategory>(categoryPageUrl, stoppingToken);

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
        var url = GetApiUrl(_productUrlFormat, categoryProductItem);
        var (success, productItem) = await TryGetProductItemFromCategoryItemAsync(categoryProductItem, url, cancellationToken);

        if(!success)
        {
            Logger.LogInformation(ImportProductLogMessages.ProductWasNotLoadedFromUrlWithError, url);
            return false;
        }

        Logger.LogInformation(ImportProductLogMessages.ProductHasBeenSuccessfullyLoadedFromUrl, productItem.Name, url);

        try
        {
            var result = await _itemHandler.HandleItem(new ImportProduct(productItem, _sourceName, Host), cancellationToken);
            Logger.LogInformation(ImportProductLogMessages.ProductFromUrlHandledWithStatusInfo, [productItem.Name, productItem.ApiUrl, result]);
        }
        catch(Exception e)
        {
            Logger.LogError(e, ImportProductLogMessages.ProductFromUrlHandlingFailed, [productItem.ApiUrl, e.Message]);
        }

        return true;
    }

    protected virtual async Task<(bool success, T? result)> TryGetFromApiUrlAsync<T>(string apiUrl, CancellationToken token)
    {
        var (success, stream) = await TryLoadFromUrlAsync(apiUrl, token);

        if (!success) return (false, default(T?));

        try
        {
            using (stream)
            {
                var item = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: token);
                stream.Close();
                return (true, item);
            }
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, ImportProductLogMessages.SerializationError, [typeof(T).Name, apiUrl, ex.Message]);
            return (false, default(T?));
        }
#if DEBUG
        catch(Exception e)
        {
            throw;
        }
#endif         
    }

    protected async Task<(bool success, TProductItem? productItem)> TryGetProductItemFromCategoryItemAsync(ICategoryProductItem categoryProductItem, string apiUrl, CancellationToken token)
    {
        var (success, productItem) = await TryGetFromApiUrlAsync<TProductItem>(apiUrl, token);

        if (!success) return (false, default(TProductItem?));
        
        productItem.Url = categoryProductItem.ItemUrl;
        productItem.Price = categoryProductItem.Price;
        productItem.Currency = categoryProductItem.Currency;
        productItem.ApiUrl = apiUrl;
        productItem.CategoryId = categoryProductItem.CategoryItemId;

        return (true, productItem);
    }

    protected abstract bool IsEndOfCategory(TCategory category);

    protected abstract string GetApiUrl(string productUrlFormat, ICategoryProductItem productItem);
}