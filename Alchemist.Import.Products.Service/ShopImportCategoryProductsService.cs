using Alchemist.Common;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Service;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace Alchemist.Import.Products.Service;

public abstract class ShopImportCategoryProductsService<TCategory, TProductItem> : ImportService
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    protected record CategoryResult(TCategory Category, int SuccessProductCount, int UnsuccessProductCount);

    protected sealed record ImportProduct(IProductItem ProductItem, IShopItem Shop) : IImportProduct
    {
    }

    private readonly IProductItemHandler _itemHandler;

    private readonly SemaphoreSlim _loaderSemaphoreSlim = new(1, 1);

    protected ConcurrentQueue<IProductShopCategory> Categories { get; }

    protected IProductShopModel ProductShopModel { get; }

    protected abstract int PageProductCount { get; }

    protected virtual int MaxUnsuccessRequestCount => 10;

    public ShopImportCategoryProductsService(ILogger logger,
        IProductShopModel shopUrlModel,
        ILoaderService loader,
        IProductItemHandler itemHandler)
        : base(logger, loader, shopUrlModel.Host)
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

    protected override async Task ProcessAsync(CancellationToken stoppingToken, CancellationTokenSource serviceStoppingToken)
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
            var categoryPageUrl = string.Format(ProductShopModel.CategoryUrl, categoryUrl, page);

            while (isEndOfCategory != true)
            {
                var categoryPageResult = await ProcessCategoryAsync(categoryPageUrl, page, category.ItemId, stoppingToken);

                if (categoryPageResult.Status == ResultStatus.Error)
                    break;

                if (categoryPageResult.Status == ResultStatus.Warning && categoryPageResult.Value?.Category == null)
                {
                    attemptsCount++;

                    if (attemptsCount == MaxUnsuccessRequestCount)
                        break;

                    continue;
                }
                else
                    attemptsCount = 0;

                var nextCategoryPageUrl = CategoryPaging.GetNextPage(categoryPageResult.Value.Category, ProductShopModel.CategoryUrl, categoryUrl, page);

                successProductCount += categoryPageResult.Value.SuccessProductCount;
                unsuccessProductCount += categoryPageResult.Value.UnsuccessProductCount;

                isEndOfCategory = IsLastPage(categoryPageResult.Value.Category, page) == true || IsEndOfCategory(categoryPageResult.Value.Category);

                categoryPageUrl = nextCategoryPageUrl;
                page++;
            }

            Logger.LogInformation(ImportProductLogMessages.CategoryCompletedInfo, [categoryUrl, successProductCount, unsuccessProductCount]);
        }
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

    protected virtual async Task<TaskResult<CategoryResult>> ProcessCategoryAsync(string categoryPageUrl, int page, int categoryItemId, CancellationToken stoppingToken)
    {
        var categoryResult = await ProcessGetCategoryAsync(categoryPageUrl, page, stoppingToken);

        if (categoryResult.Status == ResultStatus.Success)
        {
            var successCount = 0;
            foreach (var categoryProductItem in categoryResult.Value.CategoryProductItems)
            {
                categoryProductItem.CategoryItemId = categoryItemId;

                var status = await ProcessCategoryProductAsync(categoryProductItem, stoppingToken);
                if (status == ResultStatus.Success)
                    successCount++;
            }

            if (successCount == categoryResult.Value.CategoryProductItems.Length)
                Logger.LogInformation(ImportProductLogMessages.CategoryProductsForUrlWereProcessedSuccesfullInfo, [categoryPageUrl, successCount]);
            else if (successCount == 0)
                Logger.LogError(ImportProductLogMessages.CategoryProductsWereNotLoadedError, [categoryPageUrl]);
            else if (successCount < categoryResult.Value.CategoryProductItems.Length)
                Logger.LogWarning(ImportProductLogMessages.NotAllCategoryProductsForUrlWereProcessedWarning, [categoryPageUrl]);

            return TaskResult<CategoryResult>.Success(new CategoryResult(categoryResult.Value,
                successCount,
                categoryResult.Value.CategoryProductItems.Length - successCount));
        }

        if (categoryResult.Status == ResultStatus.Error)
            Logger.LogError(ImportProductLogMessages.CategoryLoadingFault, [categoryPageUrl, categoryResult.Exception.Message]);
        if (categoryResult.Status == ResultStatus.Warning)
            Logger.LogWarning(ImportProductLogMessages.CategoryNotLoadedFromUrlWarning, [categoryPageUrl, categoryResult.Exception.Message]);

        return TaskResult<CategoryResult>.FromStatus(categoryResult.Status);
    }

    protected async Task<ResultStatus> ProcessCategoryProductAsync(ICategoryProductItem categoryProductItem, CancellationToken token)
    {
        var url = GetApiUrl(categoryProductItem);
        var productItem = await ProcessUrlTaskAsync(url => GetProductItemFromCategoryItemAsync(categoryProductItem, url, token), url);

        if (productItem.Status == ResultStatus.Error)
        {
            Logger.LogError(ImportProductLogMessages.ProductWasNotLoadedFromUrlWithError,
                [categoryProductItem.Name, url, productItem.Exception?.Message ?? ""]);
        }
        else if (productItem.Status == ResultStatus.Success)
        {
            Logger.LogInformation(ImportProductLogMessages.ProductHasBeenSuccessfullyLoadedFromUrl, [productItem.Value.Name, productItem.Value.ApiUrl]);
            await HandleProductItemAsync(productItem.Value);
        }
        else if (productItem.Status == ResultStatus.Warning)
        {
            Logger.LogWarning(ImportProductLogMessages.ProductWasNotLoadedFromUrlWithWarningAndWouldBeReloaded,
                [categoryProductItem.Name, url, productItem.Exception?.Message ?? ""]);
        }

        return productItem.Status;
    }

    protected virtual async Task<UrlTaskResult<TCategory>> ProcessGetCategoryAsync(string categoryPageUrl, int page, CancellationToken token)
    {
        var categoryResult = await ProcessUrlTaskAsync(url => GetFromApiUrlAsync<TCategory>(url, token), categoryPageUrl);

        if (categoryResult.Status == ResultStatus.Success && categoryResult.Value != null
            && categoryResult.Value.CategoryProductItems == null && categoryResult is IPaginatorItem tokenCategory)
        {
            var urlWithPageToken = tokenCategory.GetPageUrl(ProductShopModel.CategoryUrl, page);
            categoryResult = await ProcessUrlTaskAsync(url => GetFromApiUrlAsync<TCategory>(url, token), urlWithPageToken);
        }

        return categoryResult;
    }

    protected async Task<ResultStatus> HandleProductItemAsync(TProductItem productItem)
    {
        var result = await ProcessUrlTaskAsync((i) => _itemHandler.HandleItem(new ImportProduct(i, ProductShopModel)), i => i.ApiUrl, productItem);

        Logger.LogInformation(ImportProductLogMessages.ProductFromUrlHandledWithStatusInfo, [productItem.Name, productItem.ApiUrl, result.Value]);

        return result.Value;
    }

    protected virtual async Task<T?> GetFromApiUrlAsync<T>(string apiUrl, CancellationToken token)
    {
        await _loaderSemaphoreSlim.WaitAsync(token);
       
        try
        {
            using var stream = await LoadFromUrlAsync(apiUrl);
            var item = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: token);
            return await Task.FromResult(item);
        }
#if DEBUG
        catch (Exception ex)    
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
            throw ex;
        }
#endif  
        finally
        {
            _loaderSemaphoreSlim.Release();
        }
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

