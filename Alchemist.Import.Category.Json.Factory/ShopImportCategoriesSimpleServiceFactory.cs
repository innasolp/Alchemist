using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Settings.Category;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.Category.Loader.Interfaces;

namespace Alchemist.Import.Factory.Category;

internal class ShopImportCategoriesSimpleServiceFactory(ILogger<ShopImportCategoriesSimpleService> logger,
    ILoaderServiceFactory browserServiceFactory,
    ICategoryItemHandler itemHandler, 
    IEnumerable<ICategoryLoaderFactory> categoryLoadStageFactories,
    IImportServiceLogFactory? logFactory = null) 
    : ShopImportCategoriesServiceFactory<ShopImportCategoriesSimpleService>(logger, browserServiceFactory, itemHandler, categoryLoadStageFactories, logFactory)
{ 

    protected override ShopImportCategoriesSimpleService CreateCategoriesService(ILogger logger, 
        string name, 
        ILoaderService loader,
        ICategoryShopImportSettings categoryShopImportSettings,
        IEnumerable<ICategoryLoader> categoryLoadStages,
        ICategoryItemHandler itemHandler, 
        object? categoryLoadData = null)
    {
        return new ShopImportCategoriesSimpleService(logger,
            name,
            loader,
            categoryShopImportSettings.CategorySourceUrl,
            categoryShopImportSettings.ShopName,
            categoryShopImportSettings.ShopUrl,
            categoryLoadStages,
            itemHandler,
            categoryLoadData);
    }
}