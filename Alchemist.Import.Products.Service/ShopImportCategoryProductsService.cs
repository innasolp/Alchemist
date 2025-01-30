using Alchemist.Product.Interfaces;
using Alchemist.Import.Service;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Text.Json;
using WebLoader.Interfaces;
using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Products.Service;

public abstract class ShopImportCategoryProductsService<TCategory, TProductItem> : ShopImportService, IShopProductImportService
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    protected readonly Queue<Tuple<string, int>> _unhandledCategoryPages = new();

    protected readonly Queue<ICategoryProductItem> _unhandledProductItemUrls = new();

    public event Microsoft.VisualStudio.Threading.AsyncEventHandler<ItemHandledEventArgs>? ItemHandled;

    protected Queue<IShopCategory> Categories { get; }

    public IShopUrlModel ShopUrlModel { get; }

    public ShopImportCategoryProductsService(ILogger logger, IShopUrlModel shopUrlModel, IWebLoader webLoader, RequestHeaders requestHeaders)
        : base(logger, webLoader, requestHeaders)
    {
        ShopUrlModel = shopUrlModel;
        Categories = new();

        ShopUrlModel.Categories.ToList().ForEach(c => Categories.Enqueue(c));
        ShopUrlModel.Categories.CollectionChanged += OnCategoriesAdd;

        ShopUrlModel.PropertyChanged += ShopUrlModelPropertyChanged;
    }

    private void ShopUrlModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (IsShopUrlInintialized() && Categories.Count == 0)
        {
            var newItems = ShopUrlModel.Categories.OfType<IShopCategory>().ToList();
            newItems.ForEach(Categories.Enqueue);
        }
    }

    private bool IsShopUrlInintialized()
    {
        return ShopUrlModel.ShopId != 0
            && ShopUrlModel.CategoryUrl != null
            && ShopUrlModel.ProductUrl != null;
    }

    private void OnCategoriesAdd(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0
            && IsShopUrlInintialized())
        {
            var newItems = e.NewItems.OfType<IShopCategory>().ToList();
            newItems.ForEach(Categories.Enqueue);
        }
    }

    public override async Task Start(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (Categories.Count > 0)
            {
                var category = Categories.Dequeue();

                int productCount = 0;
                int page = 1;

                TCategory? currentCategoryProducts = null;

                do
                {
                    await StartWebLoaderAsync(stoppingToken);

                    var currentCategoryUrl = string.Format(ShopUrlModel.CategoryUrl, category.GetCategoryForUrl(), page);

                    var categoryResult = await ProcessUrlTaskAsync((url) => ProcessCategoryProductsAsync(url, page + 1), currentCategoryUrl);
                    if (categoryResult?.Result == false)
                    {
                        _unhandledCategoryPages.Enqueue(new Tuple<string, int>(category.Category, page));
                        Logger.LogWarning($"Category {currentCategoryUrl} page {page} failed.");
                        page++;
                        continue;
                    }

                    if (categoryResult?.Result == true && categoryResult?.ProductCount == 0)
                    {
                        Logger.LogInformation($"Category {currentCategoryUrl} completed. {productCount} products handled.");
                        break;
                    }

                    if (categoryResult != null)
                    {
                        currentCategoryProducts = categoryResult.Category;
                        productCount += categoryResult.ProductCount;
                    }

                    page++;
                }
                while (productCount == 0 || productCount <= currentCategoryProducts?.TotalCount);
            }
        }
    }

    protected virtual async Task<CategoryResult<TCategory>?> ProcessCategoryProductsAsync(string currentCategoryUrl, int page)
    {
        var currentCategoryProducts = await LoadCategoryProductsAsync(currentCategoryUrl, page + 1);
        if (currentCategoryProducts == null)
            return await Task.FromResult(new CategoryResult<TCategory>(currentCategoryProducts, 0, false));


        if (IsEndOfCategory(currentCategoryProducts))
            return await Task.FromResult(new CategoryResult<TCategory>(currentCategoryProducts, 0, true));

        int productCount = 0;
        foreach (var item in currentCategoryProducts.CategoryProductItems)
        {
            //await ProcessCategoryItem(item);
            var apiUrl = GetApiUrl(item);
            
            if (!await ProcessCategoryItemAsync(item, apiUrl))
                _unhandledProductItemUrls.Enqueue(item);

            productCount++;
        }

        //todo
        //var unhandledItemUrlsCount = _unhandledProductItemUrls.Count;
        //for (int i = 0; i < unhandledItemUrlsCount; i++)
        //{
        //    var unhandledItem = _unhandledProductItemUrls.Dequeue();
        //    var apiUrl = GetApiUrl(unhandledItem);
        //    var result = await ProcessCategoryItemAsync(unhandledItem, apiUrl);
        //    if (result == ItemProcessStatus.Error)
        //        _unhandledProductItemUrls.Enqueue(unhandledItem);
        //}

        return await Task.FromResult(new CategoryResult<TCategory>(currentCategoryProducts, productCount, true));
    }

    //protected abstract Task<ItemProcessStatus> HandleCategoryItemAsync(ICategoryProductItem item, string apiUrl);

    protected virtual async Task<TProductItem?> GetShopProductItemFromApiUrlAsync(string apiUrl)
    {
        using var stream = await LoadFromUrlAsync(apiUrl);
        var product = await JsonSerializer.DeserializeAsync<TProductItem>(stream);
        stream.Close();
        return await Task.FromResult(product);
    }

    protected async Task<bool> ProcessCategoryItemAsync(ICategoryProductItem categoryProductItem, string apiUrl)
    {
        try
        {
            var productItem = await GetShopProductItemFromApiUrlAsync(apiUrl);

            if (productItem == null)
                throw new Exception($"Product item from {apiUrl} is null");

            productItem.ItemUrl = categoryProductItem.ItemUrl;
            productItem.Price = categoryProductItem.Price;
            productItem.Currency = categoryProductItem.Currency;
            productItem.ApiUrl = apiUrl;

            await OnItemHandleAsync(productItem, apiUrl, true);
            Logger.LogInformation($"{apiUrl} processed succsessfully");
            return await Task.FromResult(true);
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, $"{apiUrl} processed with error");
            return await Task.FromResult(false);
        }       
    }

    protected virtual async Task OnItemHandleAsync(IProductItem item,string apiUrl, bool status)
    {
        await ItemHandled.InvokeAsync(this, new ItemHandledEventArgs(item, apiUrl,status));
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

//public abstract class ShopImportCategoryProductsService<TCategory, TProductItem>(ILogger logger,
//    IShopUrlModel shopUrlModel,
//    IWebLoader webLoader,
//    RequestHeaders requestHeaders): ShopImportCategoryProductsService<TCategory>(logger,shopUrlModel,webLoader,requestHeaders)
//    where TCategory : class, ICategoryProducts, new()
//    where TProductItem : class, IProductItem, new()
//{
//    protected virtual async Task<TProductItem?> GetShopProductItemFromApiUrlAsync(string apiUrl)
//    {
//        using var stream = await LoadFromUrlAsync(apiUrl);
//        var product = await JsonSerializer.DeserializeAsync<TProductItem>(stream);
//        stream.Close();
//        return await Task.FromResult(product);
//    }
//    protected override Task<ItemProcessStatus> HandleCategoryItemAsync(ICategoryProductItem item, string apiUrl)
//    {
//        var shopProductModel = await GetShopProductItemFromApiUrlAsync(apiUrl);
//    }
//}
