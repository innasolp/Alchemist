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
    protected record CategoryPage(IProductShopCategoryModel Category, int Page);

    protected record CategoryProductsResult(TCategory? CategoryItem, int? Count, bool IsEnd);

    protected readonly ConcurrentQueue<CategoryPage> _unhandledCategoryPages = new();

    protected readonly ConcurrentQueue<ICategoryProductItem> _unhandledCategoryProductItems = new();

    protected readonly ConcurrentQueue<TProductItem> _unhandledProductItems = new();

    private readonly IProductItemHandler _itemHandler;

    protected Queue<IProductShopCategoryModel> Categories { get; }

    protected IProductShopModel ProductShopModel { get; }

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
            var newItems = e.NewItems.OfType<IProductShopCategoryModel>().ToList();
            newItems.ForEach(Categories.Enqueue);
        }
    }
    
    public override async Task Start(CancellationToken stoppingToken)
    {
        var unprocessedCategoriesTask = Task.Factory.StartNew(async () => await ProcessUnhandledCategoriesAsync(stoppingToken), stoppingToken);
        
        var unprocessedProductsTask = Task.Factory.StartNew(async () => await ProcessUnhandledCategoryProductsAsync(stoppingToken), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await StartWebLoaderIfNeedAsync(stoppingToken);

            if (!WebLoader.IsStarted) return;
            
            await ProcessCategoriesAsync();
        }
    }

    protected async Task ProcessCategoriesAsync()
    {
        while (Categories.Count > 0)
        {
            var category = Categories.Dequeue();

            int productCount = 0;
            int page = 1;

            var categoryUrl = category.GetCategoryForUrl();

            CategoryProductsResult? categoryResult = null;
            while (categoryResult?.IsEnd != true)
            {
                var categoryPageUrl = string.Format(ProductShopModel.CategoryUrl, categoryUrl, page);

                var result = await ProcessUrlTaskAsync((c) => ProcessCategoryPageAsync(c, category.ItemId), categoryPageUrl);                

                if (result.Result == null)
                {
                    if (result.Status == Status.Warning)
                        _unhandledCategoryPages.Enqueue(new CategoryPage(category, page));

                    page++;
                    continue;
                }

                categoryResult = result.Result;

                productCount += categoryResult.Count ?? 0;
                page++;
            }
                ;

            Logger.LogInformation($"Category {categoryUrl} completed. {productCount} products handled. {_unhandledCategoryProductItems.Count} unhandled.");
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

                var categoryUrl = unhandledCategory.Category.GetCategoryForUrl();

                var categoryPageUrl = string.Format(ProductShopModel.CategoryUrl, unhandledCategory.Category.GetCategoryForUrl(), unhandledCategory.Page);

                var categoryResult = await ProcessUrlTaskAsync((c) => ProcessCategoryPageAsync(c, unhandledCategory.Category.ItemId), categoryPageUrl);

                if (categoryResult.Status == Status.Success)
                {
                    Logger.LogInformation($"Category {categoryUrl} recompleted succsessfully. {categoryResult} product items handled.");
                }
                else if (categoryResult.Status == Status.Warning)
                {
                    _unhandledCategoryPages.Enqueue(unhandledCategory);
                }
            }
        }
    }

    protected virtual async Task<CategoryProductsResult?> ProcessCategoryPageAsync(string categoryUrl, int categoryItemId)
    {
        var currentCategoryProducts = await LoadCategoryProductsAsync(categoryUrl);

        if (currentCategoryProducts == null)
            return null;

        if (IsEndOfCategory(currentCategoryProducts))
            return await Task.FromResult(new CategoryProductsResult(currentCategoryProducts, null, true));

        int productCount = 0;
        var unprocessedProductItems = new List<ICategoryProductItem>();
        foreach (var item in currentCategoryProducts.CategoryProductItems)
        {
            item.CategoryItemId = categoryItemId;
            var productItem = await ProcessUrlTaskAsync(url => GetProductItemFromCategoryItemAsync(item, url), GetApiUrl(item));
            if (productItem.Result != null && productItem.Status == Status.Success)
            {
                productCount++;
                if (await HandleProductItemAsync(productItem.Result) == ItemProcessStatus.Error)
                    _unhandledProductItems.Enqueue(productItem.Result);
            }
            else if (productItem.Status == Status.Warning)
                _unhandledCategoryProductItems.Enqueue(item);
        }

        return await Task.FromResult(new CategoryProductsResult(currentCategoryProducts, productCount, false));
    }

    protected async Task ProcessUnhandledCategoryProductsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (_unhandledCategoryProductItems.Count > 0 && !stoppingToken.IsCancellationRequested && WebLoader.IsStarted)
            {
                if(!_unhandledCategoryProductItems.TryDequeue(out var unprocessedItem))
                    continue;

                var productItem = await ProcessUrlTaskAsync(url => GetProductItemFromCategoryItemAsync(unprocessedItem, url), GetApiUrl(unprocessedItem));
                if (productItem.Status == Status.Error)
                    continue;

                if (productItem.Status == Status.Warning)
                    _unhandledCategoryProductItems.Enqueue(unprocessedItem);
                else if (productItem.Result != null &&  await HandleProductItemAsync(productItem.Result) == ItemProcessStatus.Warning)
                    _unhandledProductItems.Enqueue(productItem.Result);
            }
        }
    }

    protected async Task ProcessUnhandledProductItemsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (_unhandledProductItems.Count > 0 && !stoppingToken.IsCancellationRequested && WebLoader.IsStarted)
            {
                if(!_unhandledProductItems.TryDequeue(out var unhandledItem))
                    continue;

                if (await HandleProductItemAsync(unhandledItem) == ItemProcessStatus.Error)
                    _unhandledProductItems.Enqueue(unhandledItem);
            }
        }
    }

    protected async Task<ItemProcessStatus> HandleProductItemAsync(TProductItem productItem)
    {
        var result = await ProcessUrlTaskAsync((i) => _itemHandler.HandleItem(i, ProductShopModel), i => i.ApiUrl, productItem);

        Logger.LogInformation($"Product {productItem.Name} {productItem.ApiUrl} handled with status {result.Result}.");

        return result.Result;
    }

    protected virtual async Task<TProductItem?> GetShopProductItemFromApiUrlAsync(string apiUrl)
    {
        using var stream = await LoadFromUrlAsync(apiUrl);
        var product = await JsonSerializer.DeserializeAsync<TProductItem>(stream);
        stream.Close();
        return await Task.FromResult(product);
    }

    protected async Task<TProductItem?> GetProductItemFromCategoryItemAsync(ICategoryProductItem categoryProductItem, string apiUrl)
    {
        var productItem = await GetShopProductItemFromApiUrlAsync(apiUrl);

        if (productItem == null)
            throw new Exception($"Product item from {apiUrl} is null");

        productItem.ItemUrl = categoryProductItem.ItemUrl;
        productItem.Price = categoryProductItem.Price;
        productItem.Currency = categoryProductItem.Currency;
        productItem.ApiUrl = apiUrl;
        productItem.CategoryId = categoryProductItem.CategoryItemId;

        return await Task.FromResult(productItem);
    }

    protected virtual async Task<TCategory?> LoadCategoryProductsAsync(string categoryUrl)
    {
        try
        {
            using var stream = await LoadFromUrlAsync(categoryUrl);
            var category = await JsonSerializer.DeserializeAsync<TCategory>(stream);
            stream.Close();
            return category;
        }
        catch (JsonException e)
        {
            throw new CategoryUrlStreamDeserializeException($"data from url {categoryUrl} has invalid format", e);
        }
        catch
        {
            throw;
        }
    }

    protected virtual async Task<Stream> LoadFromUrlAsync(string url)
    {
        var stream = await WebLoader.LoadFromUrl(url, RequestHeaders);
        return await Task.FromResult(stream);
    }

    protected abstract bool IsEndOfCategory(TCategory category);

    protected abstract string GetApiUrl(ICategoryProductItem productItem);
}

