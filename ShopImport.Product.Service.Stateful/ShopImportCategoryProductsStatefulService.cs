using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace ShopImport.Product.Service.Stateful;

public abstract class ShopImportCategoryProductsStatefulService<TCategory, TProductItem>(ILogger logger,
    ILoaderService loader,
    string url,
    IEnumerable<IProductShopCategory> shopCategories,
    IProductItemHandler itemHandler,
    string productPathFormat,
    string categoryPathFormat,
    string sourceName,
    ICategoryPaging<TCategory> categoryPaging,
    IItemSerializer<TCategory> categorySerializer,
    ImportProductServiceOptions importProductServiceOptions) : 
    ShopImportCategoryProductsService<TCategory, TProductItem>(logger,
        loader,
        url, 
        shopCategories, 
        itemHandler,
        productPathFormat,
        categoryPathFormat, 
        sourceName,
        categoryPaging,
        categorySerializer,
        importProductServiceOptions)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    private readonly ServiceState<TCategory> _serviceState = new();

    protected override async Task ProcessCategories(CancellationToken stoppingToken)
    {
        while(!Categories.IsEmpty)
        {
            if (!Categories.TryDequeue(out var productShopCategory)) continue;

            await ProcessCategoryAsync(productShopCategory, stoppingToken);
        }
    }

    protected override async Task ProcessCategoryAsync(IProductShopCategory category, CancellationToken stoppingToken)
    {
        if(_serviceState.ProductShopCategory != category)
        {
            _serviceState.Reset();
            _serviceState.Start(category);
        }        
        
        await ProcessCategoryAsync(category, _serviceState.CategoryState, stoppingToken);
    }

    protected override Task<(bool success, TCategory? result)> TryGetCategoryFromPathAsync(string dataPath, object? requestData, string categoryPath, int page, CancellationToken token)
    {
        if (_serviceState.Category != null && _serviceState.CategoryState.CategoryPath == categoryPath && _serviceState.CategoryState.Page == page)
            return Task.FromResult((true, _serviceState.Category));

       return base.TryGetCategoryFromPathAsync(dataPath, requestData, categoryPath, page, token);
    }

    protected override async Task<bool> TryProcessCategoryProductAsync(ICategoryProductItem categoryProductItem, CancellationToken cancellationToken)
    {
        if (_serviceState.CategoryProductItem == categoryProductItem) return true;

        var result = await base.TryProcessCategoryProductAsync(categoryProductItem, cancellationToken);
        if(result) _serviceState.CategoryProductItem = categoryProductItem;

        return result;
    }
}