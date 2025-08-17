using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Json;
using Alchemist.Import.Html;
using Alchemist.Test.Import.Service.Infrastructure;
using BrowserDataLoader.Interfaces;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Category.Test.Infrastructure;

public class ShopImportCategoriesTimerServiceTest(ILogger<ShopImportCategoriesTimerService> logger,
    IHtmlSearcher? htmlSearcher, 
    IWebLoader webLoader,
    IBrowserDataLoader browserDataLoader,
    ICategoryShopModel shop, 
    RequestHeaders? requestHeaders, 
    CategoryLoadOptions categoryLoadOptions,
    ICategoryItemHandler itemHandler)
    : ShopImportCategoriesTimerService(logger, htmlSearcher, webLoader, browserDataLoader, shop, requestHeaders, categoryLoadOptions, itemHandler), ITestService
{
    private string _name;
    public override string Name => _name;
    public void SetName(string name)
    {
        _name = name;
    }
}
