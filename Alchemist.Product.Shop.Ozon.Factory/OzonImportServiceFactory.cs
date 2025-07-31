using Alchemist.Import.Interfaces;
using Alchemist.Import.Logging;
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
    IEnumerable<IWebLoaderFactory> webLoaderFactories,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null,
    IPerfomanceCounter? perfomanceCounter = null)
        : ShopProductImportServiceFactory(logger, webLoaderFactories, itemHandler, logFactory, perfomanceCounter)
{
    public override Type ServiceImplementationType => typeof(OzonImportService);

    protected override IImportService Create(ILogger logger, IProductShopModel shopModel, IShopImportSettings shopImportSettings, IProductItemHandler itemHandler, IWebLoader webLoader, RequestHeaders? requestHeaders)
    {
        return new OzonImportService(logger as ILogger<OzonImportService>,
            shopModel,
            webLoader,
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
