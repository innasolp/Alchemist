using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Factory.Logging;
using Alchemist.Import.Factory.Products;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Shop.GoldApple.ImportService;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Product.Shop.GoldApple.Factory;

public class GoldappleImportServiceFactory(ILogger<GoldAppleImportService> logger,
    IEnumerable<IWebLoader> webLoaders,
    IBrowserServiceFactory browserServiceFactory,
    IProductItemHandler itemHandler, 
    IImportServiceLogFactory? logFactory = null, 
    IPerfomanceCounter? perfomanceCounter = null)
        : ShopProductImportServiceFactory(logger, webLoaders, browserServiceFactory, itemHandler, logFactory, perfomanceCounter)
{  
    public override Type ServiceImplementationType => typeof(GoldAppleImportService);    

    protected override IImportService Create(ILogger logger, IProductShopModel shopModel, IShopImportSettings shopImportSettings, IProductItemHandler itemHandler, 
        IWebLoader webLoader, IBrowserService browserService, RequestHeaders? requestHeaders)
    {
        return new GoldAppleImportService(logger as ILogger<GoldAppleImportService>, 
            shopModel,
            webLoader,
            browserService,
            requestHeaders, 
            itemHandler);
    }

    protected override ILogger GetLogger(ILogger logger, IImportServiceLogFactory importServiceLogFactory, IShopItem shopModel, IShopImportSettings shopImportSettings)
    {
        if(logger is ILogger< GoldAppleImportService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, shopModel, shopImportSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}
