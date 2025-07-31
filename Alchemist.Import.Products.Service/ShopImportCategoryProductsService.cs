using Alchemist.Import.Service;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Interfaces;
using Alchemist.Import.Products.Interfaces;
using WebLoader.Common;
using Alchemist.Common;
using System.Collections.Concurrent;
using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Products.Service;

public abstract class ShopImportCategoryProductsService<TCategory, TProductItem> : ShopImportService
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    protected record CategoryPage(IProductShopCategory Category, string Url, int Page); 

    protected sealed record ImportProduct(IProductItem ProductItem, IShopItem Shop) : IImportProduct
    {
    }

    protected readonly ConcurrentQueue<CategoryPage> _unhandledCategoryPages = new();

    protected readonly ConcurrentQueue<ICategoryProductItem> _unhandledCategoryProductItems = new();

    protected readonly ConcurrentQueue<TProductItem> _unhandledProductItems = new();

    private readonly IProductItemHandler _itemHandler;

    protected Queue<IProductShopCategory> Categories { get; }

    protected IProductShopModel ProductShopModel { get; }

    protected abstract int PageProductCount { get; }

    protected virtual int MaxUnsuccessRequestCount => 20;

    public ShopImportCategoryProductsService(ILogger logger,
        IProductShopModel shopUrlModel,
        IWebLoader webLoader,
        RequestHeaders requestHeaders,
        IProductItemHandler itemHandler)
        : base(logger, webLoader, requestHeaders)
    {
        ProductShopModel = shopUrlModel;
        _itemHandler = itemHandler;
        Categories = new();

        ProductShopModel.Categories.ToList().ForEach(c => Categories.Enqueue(c));
        ProductShopModel.Categories.CollectionChanged += OnCategoriesAdd;
    }

    private void OnCategoriesAdd(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
        {
            var newItems = e.NewItems.OfType<IProductShopCategory>().ToList();
            newItems.ForEach(Categories.Enqueue);
            foreach (var item in newItems)            
                Logger.LogInformation(ImportProductLogMessages.NewCategoryIsEnqueued, item.GetCategoryUrl());            
        }
    }

    public override async Task Start(CancellationToken stoppingToken)
    {
        var unprocessedCategoriesTask = Task.Factory.StartNew(async () => await ProcessUnhandledCategoriesAsync(stoppingToken),
            stoppingToken, 
            TaskCreationOptions.None,
            TaskScheduler.Default);

        var unprocessedProductsTask = Task.Factory.StartNew(async () => await ProcessUnhandledCategoryProductsAsync(stoppingToken), 
            stoppingToken, 
            TaskCreationOptions.None,
            TaskScheduler.Default);

        await base.Start(stoppingToken);        
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        while (Categories.Count > 0 && !stoppingToken.IsCancellationRequested)
        {
            var category = Categories.Dequeue();

            int productCount = 0;
            int page = 1;
            var unsuccessRequestCount = 0;            

            bool? isEnd = null;

            var categoryUrl = category.GetCategoryUrl();
            var categoryPageUrl = string.Format(ProductShopModel.CategoryUrl, categoryUrl, page); 
            
            while (isEnd != true)
            {
                var categoryResult = await GetCategoryAsync(categoryPageUrl, page, stoppingToken);
                
                if (categoryResult.Status == ResultStatus.Cancelled)
                    return;

                if (categoryResult.Status == ResultStatus.Error)
                {
                    Logger.LogError(ImportProductLogMessages.CategoryLoadingFault, [categoryPageUrl, categoryResult.Exception.Message]);
                    break;
                }

                var nextCategoryPageUrl = categoryResult.Value.GetNextCategoryPage(ProductShopModel.CategoryUrl, categoryUrl, page);

                if (categoryResult.Status == ResultStatus.Warning)
                {
                    _unhandledCategoryPages.Enqueue(new CategoryPage(category, categoryResult.Url, page));
                    Logger.LogWarning(ImportProductLogMessages.CategoryNotLoadedFromUrlWarning, [categoryPageUrl, categoryResult.Exception.Message]);
                    unsuccessRequestCount++;                    

                    if (unsuccessRequestCount > MaxUnsuccessRequestCount) break;

                    categoryPageUrl = nextCategoryPageUrl;
                    page++;

                    continue;
                }                

                var processCategoryProductsResult = await ProcessUrlTaskAsync((c) => ProcessCategoryProductsAsync(categoryResult.Value.CategoryProductItems, category.ItemId, stoppingToken), categoryResult.Url);

                if (processCategoryProductsResult.Status == ResultStatus.Cancelled)
                    return;

                LogCategoryProductsResult(processCategoryProductsResult.Value, categoryResult.Value.CategoryProductItems.Length, categoryPageUrl,
                    ImportProductLogMessages.CategoryProductsWereNotLoadedError,
                    ImportProductLogMessages.NotAllCategoryProductsForUrlWereProcessedWarning,
                    ImportProductLogMessages.CategoryProductsForUrlWereProcessedSuccesfullInfo);

                productCount += processCategoryProductsResult.Value;                        

                var currentTotalCount = categoryResult.Value?.TotalCount;
                if (currentTotalCount != null)
                {
                    var totalPageCount = currentTotalCount % PageProductCount > 0
                        ? currentTotalCount / PageProductCount + 1
                        : currentTotalCount / PageProductCount;

                    if (page >= totalPageCount)
                        break;
                }

                isEnd = IsEndOfCategory(categoryResult.Value);

                categoryPageUrl = nextCategoryPageUrl;
                page++;
            }                

            Logger.LogInformation(ImportProductLogMessages.CategoryCompletedInfo, [categoryUrl, productCount, _unhandledCategoryProductItems.Count]);
        }
    }

    protected virtual void LogCategoryProductsResult(int processedCount, int totalCount, string url, string errorFormat, string warningFormat, string successFormat)
    {
        if (processedCount == 0)
            Logger.LogError(errorFormat, url);
        else if (processedCount < totalCount)
            Logger.LogWarning(warningFormat,
                [url, processedCount, totalCount]);
        else Logger.LogInformation(successFormat, url, processedCount);
    }

    protected async Task ProcessUnhandledCategoriesAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (_unhandledCategoryPages.Count > 0 && !stoppingToken.IsCancellationRequested && WebLoader.IsStarted)
            {
                if (!_unhandledCategoryPages.TryDequeue(out var unhandledCategory))
                    continue;
                
                var categoryResult = await GetCategoryAsync(unhandledCategory.Url, unhandledCategory.Page, stoppingToken);

                if (categoryResult.Status != ResultStatus.Success)
                {
                    if (categoryResult.Status == ResultStatus.Cancelled)
                        return;

                    if (categoryResult.Status == ResultStatus.Warning)
                    {
                        _unhandledCategoryPages.Enqueue(unhandledCategory);
                        Logger.LogWarning(ImportProductLogMessages.CategoryNotReloadedFromUrlWarning, [categoryResult.Url, categoryResult.Exception.Message]);
                    }
                    else if (categoryResult.Status == ResultStatus.Error)
                        Logger.LogError(ImportProductLogMessages.CategoryReloadingFailed, [categoryResult.Url, categoryResult.Exception.Message]);
                    
                    continue;
                }

                var result = await ProcessUrlTaskAsync((c) => ProcessCategoryProductsAsync(categoryResult.Value.CategoryProductItems,
                    unhandledCategory.Category.ItemId, stoppingToken), categoryResult.Url);

                if (result.Status == ResultStatus.Cancelled)
                    return;

                LogCategoryProductsResult(result.Value, categoryResult.Value.CategoryProductItems.Length, categoryResult.Url,
                    ImportProductLogMessages.CategoryProductsWereNotLoadedError,
                    ImportProductLogMessages.NotAllCategoryProductsForUrlWereReprocessedWarning,
                    ImportProductLogMessages.CategoryProductsForUrlWereReprocessedSuccesfullInfo);                
            }
        }
    }

    protected virtual async Task<UrlTaskResult<TCategory>> GetCategoryAsync(string categoryPageUrl, int page, CancellationToken token)
    {
        var categoryResult = await ProcessUrlTaskAsync(url=> GetFromApiUrlAsync<TCategory>(url, token), categoryPageUrl);

        if (categoryResult.Status == ResultStatus.Success && categoryResult.Value != null
            && categoryResult.Value.CategoryProductItems == null && categoryResult is IPaginatorItem tokenCategory)
        {
            var urlWithPageToken = tokenCategory.GetPageUrl(ProductShopModel.CategoryUrl, page);
            categoryResult = await ProcessUrlTaskAsync(url => GetFromApiUrlAsync<TCategory>(url, token), urlWithPageToken);            
        }

        return categoryResult;
    }

    protected virtual async Task<int> ProcessCategoryProductsAsync(IEnumerable<ICategoryProductItem> categoryProductItems, int categoryItemId, CancellationToken token)
    {
        int productCount = 0;
        var unprocessedProductItems = new List<ICategoryProductItem>();
        foreach (var item in categoryProductItems)
        {
            //todo
            item.CategoryItemId = categoryItemId;

            var url = GetApiUrl(item);
            var productItem = await ProcessUrlTaskAsync(url => GetProductItemFromCategoryItemAsync(item, url, token), url);
           
            if(productItem.Status == ResultStatus.Cancelled)
                token.ThrowIfCancellationRequested();

            if(productItem.Status == ResultStatus.Error)
            {
                Logger.LogError(ImportProductLogMessages.ProductWasNotLoadedFromUrlWithError, [item.Name, url, productItem.Exception?.Message ?? ""]);
                continue;
            }

            if (productItem.Value != null && productItem.Status == ResultStatus.Success)
            {
                productCount++;
                Logger.LogInformation(ImportProductLogMessages.ProductHasBeenSuccessfullyLoadedFromUrl, [productItem.Value.Name, productItem.Value.ApiUrl]);
                if (await HandleProductItemAsync(productItem.Value) == ResultStatus.Error)
                    _unhandledProductItems.Enqueue(productItem.Value);
            }
            else if (productItem.Status == ResultStatus.Warning)
            {
                _unhandledCategoryProductItems.Enqueue(item);
                Logger.LogWarning(ImportProductLogMessages.ProductWasNotLoadedFromUrlWithWarningAndWouldBeReloaded,
                    [item.Name, url, productItem.Exception?.Message ?? ""]);
            }
        }

        return await Task.FromResult(productCount);
    }

    protected async Task ProcessUnhandledCategoryProductsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (_unhandledCategoryProductItems.Count > 0 && !stoppingToken.IsCancellationRequested && WebLoader.IsStarted)
            {
                if (!_unhandledCategoryProductItems.TryDequeue(out var unprocessedItem))
                    continue;

                var productItem = await ProcessUrlTaskAsync(url => GetProductItemFromCategoryItemAsync(unprocessedItem, url, stoppingToken), 
                                                                    GetApiUrl(unprocessedItem));

                if (productItem.Status == ResultStatus.Cancelled)
                    return;

                if (productItem.Status == ResultStatus.Error)
                    continue;

                if (productItem.Status == ResultStatus.Warning)
                    _unhandledCategoryProductItems.Enqueue(unprocessedItem);
                else if (productItem.Value != null && await HandleProductItemAsync(productItem.Value) == ResultStatus.Warning)
                    _unhandledProductItems.Enqueue(productItem.Value);
            }
        }
    }

    protected async Task ProcessUnhandledProductItemsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (_unhandledProductItems.Count > 0 && !stoppingToken.IsCancellationRequested && WebLoader.IsStarted)
            {
                if (!_unhandledProductItems.TryDequeue(out var unhandledItem))
                    continue;

                if (await HandleProductItemAsync(unhandledItem) == ResultStatus.Error)
                    _unhandledProductItems.Enqueue(unhandledItem);
            }
        }
    }

    protected async Task<ResultStatus> HandleProductItemAsync(TProductItem productItem)
    {
        var result = await ProcessUrlTaskAsync((i) => _itemHandler.HandleItem(new ImportProduct(i, ProductShopModel)), i => i.ApiUrl, productItem);

        Logger.LogInformation(ImportProductLogMessages.ProductFromUrlHandledWithStatusInfo, [productItem.Name, productItem.ApiUrl, result.Value]);

        return result.Value;
    }

    protected virtual async Task<T?> GetFromApiUrlAsync<T>(string apiUrl, CancellationToken token)
    {
        using var stream = await LoadFromUrlAsync(apiUrl);
        var product = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: token);
        stream.Close();
        return await Task.FromResult(product);
    }

    protected async Task<TProductItem?> GetProductItemFromCategoryItemAsync(ICategoryProductItem categoryProductItem, string apiUrl, CancellationToken token)
    {
        var productItem = await GetFromApiUrlAsync<TProductItem>(apiUrl, token);

        if (productItem == null)
            throw new Exception(string.Format(ImportProductLogMessages.ProductFromUrlIsNullError, apiUrl));

        productItem.Url = categoryProductItem.ItemUrl;
        productItem.Price = categoryProductItem.Price;
        productItem.Currency = categoryProductItem.Currency;
        productItem.ApiUrl = apiUrl;
        productItem.CategoryId = categoryProductItem.CategoryItemId;

        return await Task.FromResult(productItem);
    }

    protected abstract bool IsEndOfCategory(TCategory category);

    protected abstract string GetApiUrl(ICategoryProductItem productItem);
}

