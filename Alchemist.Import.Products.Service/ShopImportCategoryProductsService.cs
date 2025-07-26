using Alchemist.Import.Service;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Interfaces;
using Alchemist.Import.Products.Interfaces;
using WebLoader.Common;
using Alchemist.Common;
using System.Collections.Concurrent;

namespace Alchemist.Import.Products.Service;

public abstract class ShopImportCategoryProductsService<TCategory, TProductItem> : ShopImportService
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    protected record CategoryPage(IProductShopCategory Category, int Page);

    protected record CategoryProductsResult(TCategory? CategoryItem, int? Count, bool IsEnd);

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

            var categoryUrl = category.GetCategoryUrl();

            bool? isEnd = null;
            while (isEnd != true)
            {
                var categoryPageUrl = string.Format(ProductShopModel.CategoryUrl, categoryUrl, page);

                var result = await ProcessUrlTaskAsync((c) => ProcessCategoryPageAsync(c, category.ItemId, stoppingToken), categoryPageUrl);
                if (result.Status == ResultStatus.Cancelled)
                    return;

                if (result.Status == ResultStatus.Warning)
                {
                    _unhandledCategoryPages.Enqueue(new CategoryPage(category, page));
                    Logger.LogWarning(ImportProductLogMessages.CategoryProcessWarning, [categoryPageUrl, result.Exception.Message]);
                    unsuccessRequestCount++;
                }
                else if (result.Status == ResultStatus.Error)
                {
                    Logger.LogError(ImportProductLogMessages.CategoryProcessFault, [categoryPageUrl, result.Exception.Message]);
                    break;
                }
                
                if (unsuccessRequestCount > MaxUnsuccessRequestCount)                
                    break;        
                
                if (result.Value == null)
                {
                    page++;
                    continue;
                }

                productCount += result.Value.Count ?? 0;

                        

                var currentTotalCount = result.Value?.CategoryItem?.TotalCount;
                if (currentTotalCount != null)
                {
                    var totalPageCount = currentTotalCount % PageProductCount > 0
                        ? currentTotalCount / PageProductCount + 1
                        : currentTotalCount / PageProductCount;

                    if (page >= totalPageCount)
                        break;
                }                

                isEnd = result.Value.IsEnd;
                
                page++;                
            }                

            Logger.LogInformation(ImportProductLogMessages.CategoryCompletedInfo, [categoryUrl, productCount, _unhandledCategoryProductItems.Count]);
        }
    }

    protected async Task ProcessUnhandledCategoriesAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (_unhandledCategoryPages.Count > 0 && !stoppingToken.IsCancellationRequested && WebLoader.IsStarted)
            {
                if (!_unhandledCategoryPages.TryDequeue(out var unhandledCategory))
                    continue;

                var categoryUrl = unhandledCategory.Category.GetCategoryUrl();

                var categoryPageUrl = string.Format(ProductShopModel.CategoryUrl, unhandledCategory.Category.GetCategoryUrl(), unhandledCategory.Page);

                var categoryResult = await ProcessUrlTaskAsync((c) => ProcessCategoryPageAsync(c, unhandledCategory.Category.ItemId, stoppingToken), categoryPageUrl);
                
                if (categoryResult.Status == ResultStatus.Cancelled)
                    return;

                if (categoryResult.Status == ResultStatus.Success)
                {
                    Logger.LogInformation(ImportProductLogMessages.CategoryRecompletedInfo, [categoryUrl, categoryResult.Value.Count]);
                }
                else if (categoryResult.Status == ResultStatus.Warning)
                {
                    _unhandledCategoryPages.Enqueue(unhandledCategory);
                    Logger.LogWarning(ImportProductLogMessages.CategoryRecompletedWarning, [categoryUrl, categoryResult.Exception.Message]);
                }
                else
                    Logger.LogError(ImportProductLogMessages.CategoryRehandlingFailed, [categoryUrl, categoryResult.Exception.Message]);
            }
        }
    }

    protected virtual async Task<CategoryProductsResult?> ProcessCategoryPageAsync(string categoryUrl, int categoryItemId, CancellationToken token)
    {
        var currentCategoryProducts = await GetFromApiUrlAsync<TCategory>(categoryUrl, token);

        if (currentCategoryProducts == null)
            return null;

        if (IsEndOfCategory(currentCategoryProducts))
            return await Task.FromResult(new CategoryProductsResult(currentCategoryProducts, null, true));

        int productCount = 0;
        var unprocessedProductItems = new List<ICategoryProductItem>();
        foreach (var item in currentCategoryProducts.CategoryProductItems)
        {
            item.CategoryItemId = categoryItemId;
            var productItem = await ProcessUrlTaskAsync(url => GetProductItemFromCategoryItemAsync(item, url, token), GetApiUrl(item));
           
            if(productItem.Status == ResultStatus.Cancelled)
                token.ThrowIfCancellationRequested();

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
                    [item.Name, item.ItemUrl, productItem.Exception?.Message ?? ""]);
            }
        }

        return await Task.FromResult(new CategoryProductsResult(currentCategoryProducts, productCount, false));
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
        var result = await ProcessUrlTaskAsync((i) => _itemHandler.HandleItem(i, ProductShopModel), i => i.ApiUrl, productItem);

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

