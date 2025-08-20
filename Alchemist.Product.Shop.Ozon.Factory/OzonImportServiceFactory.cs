using Alchemist.Import.BrowserService.Factory;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Logging.Factory;
using Alchemist.Import.Products.Factory;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Shop.Ozon.ImportService;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Product.Shop.Ozon.Factory;

public class OzonImportServiceFactory(ILogger<OzonImportService> logger,
    IEnumerable<IWebLoader> webLoaders,
   IBrowserServiceFactory browserServiceFactory,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null,
    IPerfomanceCounter? perfomanceCounter = null)
        : ShopProductImportServiceFactory(logger, webLoaders, browserServiceFactory, itemHandler, logFactory, perfomanceCounter)
{
    public override Type ServiceImplementationType => typeof(OzonImportService);

    protected override IImportService Create(ILogger logger, IProductShopModel shopModel, IShopImportSettings shopImportSettings,
        IProductItemHandler itemHandler, IWebLoader webLoader, IBrowserService browserService, RequestHeaders? requestHeaders)
    {
        return new OzonImportService(logger as ILogger<OzonImportService>,
            shopModel,
            webLoader,
            browserService,
            requestHeaders, 
            itemHandler);
    }

    protected override ILogger GetLogger(ILogger logger, IImportServiceLogFactory importServiceLogFactory, IShopItem shopModel, IShopImportSettings shopImportSettings)
    {
        if (logger is ILogger<OzonImportService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, shopModel, shopImportSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}
