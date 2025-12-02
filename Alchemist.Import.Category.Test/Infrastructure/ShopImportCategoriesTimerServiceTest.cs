using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Json;
using Import.Html;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Category.Test.Infrastructure;

public class ShopImportCategoriesTimerServiceTest(ILogger<ShopImportCategoriesTimerService> logger,
    string name,
    IHtmlSearcher? htmlSearcher, 
    ILoaderService loader,
    ICategoryShopModel shop, 
    CategoryLoadOptions categoryLoadOptions,
    ICategoryItemHandler itemHandler)
    : ShopImportCategoriesTimerService(logger, name, htmlSearcher, loader, shop.CategorySourceUrl, shop.SourceName, shop.SourceUrl,  categoryLoadOptions, itemHandler)
{
}
