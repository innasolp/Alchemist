using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.KeyHash;
using ShopImport.ServiceState;

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
    ImportProductServiceOptions importProductServiceOptions,
    IKeyHasher keyHasher,
    IServiceStateRepository serviceStateRepository) : 
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
    private readonly ServiceStateWorker<TCategory> _serviceStateWorker = new(serviceStateRepository);

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        var serviceStateKeyObject = new
        {
            Name,
            SourceName,
            Url = Host,
            ProductPathFormat,
            СategoryPathFormat,
            StartCategory = Categories.Count > 0
                ? new { Categories.First.Value.Category, Categories.First.Value.ItemId, Categories.First.Value.Path }
                : new { Category = "", ItemId = 0, Path = "" }
        };

        var serviceStateKey = keyHasher.Hash(serviceStateKeyObject);
        _serviceStateWorker.Initialize(serviceStateKey);

        await base.ProcessAsync(stoppingToken);
    }

    protected override async Task ProcessCategories(CancellationToken stoppingToken)
    {
        while(Categories.Count > 0)
        {
            if(Categories.First == null)
            {
                Categories.RemoveFirst();
                continue;
            }
            
            var productShopCategory = Categories.First.Value;
            Categories.RemoveFirst();

            try
            {
                await ProcessCategoryAsync(productShopCategory, stoppingToken);
            }
            catch(OperationCanceledException)
            {
                Categories.AddFirst(productShopCategory);
                throw;
            }
        }
    }

    protected override async Task ProcessCategoryAsync(IProductShopCategory category, CancellationToken stoppingToken)
    {
        await _serviceStateWorker.SetProductShopCategoryIfNeedAsync(category, stoppingToken);
        
        await ProcessCategoryAsync(category, _serviceStateWorker.CategoryState, stoppingToken);
    }

    protected override async Task<(bool success, TCategory? result)> TryGetCategoryFromPathAsync(string dataPath, object? requestData, string categoryPath, int page, CancellationToken token)
    {
        if(_serviceStateWorker.IsCategoryCurrent(categoryPath, page))
            return (true, _serviceStateWorker.Category);

       var (success, result) = await base.TryGetCategoryFromPathAsync(dataPath, requestData, categoryPath, page, token);

        if (success && result != null)        
            await _serviceStateWorker.SaveCategoryAsync(result, token);        

        return (success, result);
    }

    protected override async Task<bool> TryProcessCategoryProductAsync(ICategoryProductItem categoryProductItem, CancellationToken cancellationToken)
    {
        if(_serviceStateWorker.IsCategoryProductItemCurrent(categoryProductItem))  
            return true;

        var result = await base.TryProcessCategoryProductAsync(categoryProductItem, cancellationToken);
        
        if (result)        
            await _serviceStateWorker.SaveCurrentCategoryProductItemAsync(categoryProductItem, cancellationToken);       

        return result;
    }
}