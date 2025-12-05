using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Alchemist.Import.Category.Service.Json;
using Import.Html;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Category.Test.Infrastructure;

public class ShopImportCategoriesTimerServiceTest(ILogger<ShopImportCategoriesJsonTimerService> logger,
    string name,
    IHtmlSearcher? htmlSearcher, 
    ILoaderService loader,
    ICategoryShopModel shop, 
    CategoryLoadOptions categoryLoadOptions,
    ICategoryItemHandler itemHandler)
    : ShopImportCategoriesJsonTimerService(logger, name, htmlSearcher, loader, shop.CategorySourceUrl, shop.SourceName, shop.SourceUrl,  categoryLoadOptions, itemHandler)
{
}