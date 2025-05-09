using Alchemist.Import.Service;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Text.Json;
using WebLoader.Interfaces;
using Alchemist.Import.Products.Interfaces;
using System.ComponentModel;
using WebLoader.Common;
using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Products.Service;

public abstract class ShopImportCategoryProductsService<TCategory, TProductItem> : ShopImportService
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    protected readonly Queue<Tuple<string, int>> _unhandledCategoryPages = new();

    protected readonly Queue<ICategoryProductItem> _unhandledProductItemUrls = new();

    private readonly IItemHandler _itemHandler;

    protected Queue<IProductShopCategoryModel> Categories { get; }

    protected IProductShopModel ProductShopModel { get; }    

    public ShopImportCategoryProductsService(ILogger logger,
        IProductShopModel shopUrlModel,
        IWebLoader webLoader, 
        RequestHeaders requestHeaders,
        IItemHandler itemHandler)
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
        while (!stoppingToken.IsCancellationRequested)
        {
            await StartWebLoaderIfNeedAsync(stoppingToken);

            if (!WebLoader.IsStarted) return;

            while (Categories.Count > 0)
            {
                var category = Categories.Dequeue();

                int productCount = 0;
                int page = 1;

                TCategory? currentCategoryProducts = null;

                var categoryUrl = category.GetCategoryForUrl();

                do
                {
                    var categoryResult = await ProcessUrlTaskAsync((i) => ProcessCategoryProductsAsync(i, page + 1), i => categoryUrl, category);

                    if (categoryResult == null || categoryResult?.Result == false)
                    {
                        _unhandledCategoryPages.Enqueue(new Tuple<string, int>(category.Category, page));
                        page++;
                        continue;
                    }

                    currentCategoryProducts = categoryResult.Category;
                    productCount += categoryResult.ProductCount;

                    if (categoryResult?.Result == true && categoryResult?.ProductCount == 0)
                    {
                        Logger.LogInformation($"Category {categoryUrl} completed. {productCount} products handled.");
                        break;
                    }

                    page++;
                }
                while (productCount == 0 || productCount <= currentCategoryProducts?.TotalCount);
            }
        }
    }

    protected virtual async Task<CategoryResult<TCategory>?> ProcessCategoryProductsAsync(IProductShopCategoryModel category, int page)
    {
        var currentCategoryUrl = string.Format(ProductShopModel.CategoryUrl, category.GetCategoryForUrl(), page);

        var currentCategoryProducts = await LoadCategoryProductsAsync(currentCategoryUrl, page + 1);
        if (currentCategoryProducts == null)
            throw new WarningException($"Category {currentCategoryUrl} page {page} failed.");


        if (IsEndOfCategory(currentCategoryProducts))
            return await Task.FromResult(new CategoryResult<TCategory>(currentCategoryProducts, 0, true));

        int productCount = 0;
        foreach (var item in currentCategoryProducts.CategoryProductItems)
        {
            var apiUrl = GetApiUrl(item);

            var productItem = await ProcessUrlTaskAsync((url) => ProcessCategoryItemAsync(item, url), apiUrl);
            if (productItem == null)
            {
                _unhandledProductItemUrls.Enqueue(item);
                continue;
            }

            productItem.CategoryId = category.ItemId;

            await ProcessUrlTaskAsync((url) => OnItemHandleAsync(productItem, url, true), apiUrl);

            productCount++;
        }

        return await Task.FromResult(new CategoryResult<TCategory>(currentCategoryProducts, productCount, true));
    }

    protected virtual async Task<TProductItem?> GetShopProductItemFromApiUrlAsync(string apiUrl)
    {
        using var stream = await LoadFromUrlAsync(apiUrl);
        var product = await JsonSerializer.DeserializeAsync<TProductItem>(stream);
        stream.Close();
        return await Task.FromResult(product);
    }

    protected async Task<TProductItem?> ProcessCategoryItemAsync(ICategoryProductItem categoryProductItem, string apiUrl)
    {
        var productItem = await GetShopProductItemFromApiUrlAsync(apiUrl);

        if (productItem == null)
            throw new Exception($"Product item from {apiUrl} is null");

        productItem.ItemUrl = categoryProductItem.ItemUrl;
        productItem.Price = categoryProductItem.Price;
        productItem.Currency = categoryProductItem.Currency;
        productItem.ApiUrl = apiUrl;

        return await Task.FromResult(productItem);
    }

    protected virtual async Task OnItemHandleAsync(IProductItem item, string apiUrl, bool status)
    {
        //todo
        item.ApiUrl = apiUrl;

        await _itemHandler.HandleItem(item, ProductShopModel);
    }

    protected virtual async Task<TCategory?> LoadCategoryProductsAsync(string categoryUrl, int page)
    {
        using var stream = await LoadFromUrlAsync(categoryUrl);

        try
        {
            return await JsonSerializer.DeserializeAsync<TCategory>(stream);
        }
        catch (JsonException e)
        {
            throw new CategoryUrlStreamDeserializeException($"data from url {categoryUrl} has invalid format", e);
        }
        finally
        { stream.Close(); }
    }

    protected virtual async Task<Stream> LoadFromUrlAsync(string url)
    {
        var stream = await WebLoader.LoadFromUrl(url);
        return await Task.FromResult(stream);
    }

    protected abstract bool IsEndOfCategory(TCategory category);

    protected abstract string GetApiUrl(ICategoryProductItem productItem);
}

