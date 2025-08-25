using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Factory.Logging;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Service.Factory.Abstractions;
using Alchemist.Import.Settings.Interfaces;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Factory.Products;

public abstract class ShopProductImportServiceFactory(ILogger logger,
    IEnumerable<IWebLoader> webLoaders,
    IBrowserServiceFactory browserServiceFactory,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null,
    IPerfomanceCounter? perfomanceCounter = null) : ShopImportServiceFactory(logger, webLoaders, browserServiceFactory, logFactory, perfomanceCounter)
{
    private readonly IProductItemHandler _itemHandler = itemHandler;


    protected override IImportService Create(ILogger logger, 
        IShopItem shopModel, 
        IShopImportSettings shopImportSettings,
        IWebLoader webLoader,
        IBrowserService browserService,
        RequestHeaders? requestHeaders)
    {
        return Create(logger, shopModel as IProductShopModel, shopImportSettings, _itemHandler, webLoader, browserService, requestHeaders);
    }

    protected abstract IImportService Create(ILogger logger,
        IProductShopModel shopModel,
        IShopImportSettings shopImportSettings,
        IProductItemHandler productItemHandler, 
        IWebLoader webLoader,
        IBrowserService browserService,
        RequestHeaders? requestHeaders);
}
