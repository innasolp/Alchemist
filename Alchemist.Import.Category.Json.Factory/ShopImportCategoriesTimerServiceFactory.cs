using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Category;
using Alchemist.Import.Settings.Extensions;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.Category.Loader.Interfaces;

namespace Alchemist.Import.Factory.Category;

internal class ShopImportCategoriesTimerServiceFactory(ILogger<ShopImportCategoriesTimerService> logger, 
    ILoaderServiceFactory browserServiceFactory, 
    ICategoryItemHandler itemHandler, 
    IEnumerable<ICategoryLoaderFactory> categoryLoadStageFactories,
    IImportServiceLogFactory? logFactory = null) 
    : ShopImportCategoriesServiceFactory<ShopImportCategoriesTimerService>(logger, browserServiceFactory, itemHandler, categoryLoadStageFactories, logFactory)
{
    protected override ShopImportCategoriesTimerService CreateCategoriesService(ILogger logger, 
        string name, 
        ILoaderService loader, 
        ICategoryShopImportSettings categoryShopImportSettings, 
        IEnumerable<ICategoryLoader> categoryLoadStages, 
        ICategoryItemHandler itemHandler, 
        object? categoryLoadData = null)
    {
        var categoryImportOptionsService = categoryShopImportSettings.GetService(nameof(TimerOptions));
        var categoryImportOptions = categoryImportOptionsService?.GetServiceValue<TimerOptions>();

        return new ShopImportCategoriesTimerService(logger,
            name,
            loader,
            categoryShopImportSettings.CategorySourceUrl,
            categoryShopImportSettings.ShopName,
            categoryShopImportSettings.ShopUrl,
            categoryLoadStages,
            itemHandler,
            categoryImportOptions?.SecondsInterval,
            categoryLoadData);
    }
}