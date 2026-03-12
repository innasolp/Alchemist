using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Import.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.Category.Loader.Interfaces;

namespace Alchemist.Import.CategoryService.Test.Infrastructure;

public class ShopImportCategoriesTimerServiceTest(ILogger<ShopImportCategoriesService> logger,
    string name,
    ILoaderService loader,
    ICategoryShopModel shop, 
    IEnumerable<ICategoryLoader> categoryLoadStages,
    int timerSecondsInterval,
    ICategoryItemHandler itemHandler)
    : ShopImportCategoriesTimerService(logger, name, loader, shop.CategorySourceUrl, shop.SourceName, shop.SourceUrl, categoryLoadStages, itemHandler,  timerSecondsInterval)
{
}