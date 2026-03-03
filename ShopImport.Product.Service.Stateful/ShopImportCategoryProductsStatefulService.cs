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

    protected virtual object GetServiceStateKeyObject()
    {
        return new
                {
                    Name,
                    SourceName,
                    Url = Host,
                    ProductPathFormat,
                    СategoryPathFormat,
                    ItemHandlerType = ItemHandler.GetType().Name
                };
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        var serviceStateKeyObject = GetServiceStateKeyObject();

        var serviceStateKey = keyHasher.Hash(serviceStateKeyObject);
        await _serviceStateWorker.Start(serviceStateKey, stoppingToken);

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

            IProductShopCategory productShopCategory;
            if (_serviceStateWorker.ProductShopCategory == null)
            {
                productShopCategory = Categories.First.Value;
                Categories.RemoveFirst();
            }
            else
            {
                productShopCategory = _serviceStateWorker.ProductShopCategory;
                RemovePreviousCategoriesIfNeed(productShopCategory);
                Logger.LogInformation(ImportProductStatefulLogMessages.CategoryWasLoadedFromState, productShopCategory.Path);
            }

            await ProcessCategoryAsync(productShopCategory, stoppingToken);            
        }
    }

    private void RemovePreviousCategoriesIfNeed(IProductShopCategory productShopCategory)
    {
        var element = Categories.FirstOrDefault(c => c.ItemId == productShopCategory.ItemId
            && c.Path == productShopCategory.Path
            && c.Category == productShopCategory.Category);

        if (element == null) return;

        var node = Categories.Find(element);
        while (node?.Previous != null)
        {
            Categories.RemoveFirst();
        }
    }

    protected override async Task ProcessCategoryAsync(IProductShopCategory category, CancellationToken stoppingToken)
    {
        await _serviceStateWorker.SetProductShopCategoryIfNeedAsync(category, stoppingToken);

        await ProcessCategoryAsync(category, _serviceStateWorker.CategoryProcessState, stoppingToken);

        await _serviceStateWorker.ResetAsync(stoppingToken);
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
        if(_serviceStateWorker.IsCategoryProductItemHandled(categoryProductItem))  
            return true;

        await _serviceStateWorker.SetCurrentCategoryProductItemAsync(categoryProductItem, cancellationToken);

        var result = await base.TryProcessCategoryProductAsync(categoryProductItem, cancellationToken);
        
        if (result)        
            await _serviceStateWorker.SaveHandledCategoryProductItemAsync(categoryProductItem, cancellationToken);

        await _serviceStateWorker.ResetCategoryAsync(cancellationToken);

        return result;
    }

    //todo
    //protected override async Task CloseAsync()
    //{
    //    await _serviceStateWorker.CloseAsync();
    //    await _serviceStateWorker.DisposeAsync();
    //    await base.CloseAsync();
    //}
}