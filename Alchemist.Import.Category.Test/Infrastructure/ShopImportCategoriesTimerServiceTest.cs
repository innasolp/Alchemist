using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Json;
using Alchemist.Import.Html;
using Alchemist.Import.Interfaces;
using Alchemist.Test.Import.Service.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Category.Test.Infrastructure;

public class ShopImportCategoriesTimerServiceTest(ILogger<ShopImportCategoriesTimerService> logger,
    IHtmlSearcher? htmlSearcher, 
    ILoaderService loader,
    ICategoryShopModel shop, 
    CategoryLoadOptions categoryLoadOptions,
    ICategoryItemHandler itemHandler)
    : ShopImportCategoriesTimerService(logger, htmlSearcher, loader, shop,  categoryLoadOptions, itemHandler), ITestService
{
    private string _name;
    public override string Name => _name;
    public void SetName(string name)
    {
        _name = name;
    }
}
