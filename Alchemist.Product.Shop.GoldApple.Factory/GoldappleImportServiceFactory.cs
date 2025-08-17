using Alchemist.Import.Interfaces;
using Alchemist.Import.Logging;
using Alchemist.Import.Products.Factory;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Shop.GoldApple.ImportService;
using BrowserDataLoader.Interfaces;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Product.Shop.GoldApple.Factory;

public class GoldappleImportServiceFactory(ILogger<GoldAppleImportService> logger,
    IEnumerable<IWebLoader> webLoaders,
    IEnumerable<IBrowserDataLoader> browserDataLoaders,
    IProductItemHandler itemHandler, 
    IImportServiceLogFactory? logFactory = null, 
    IPerfomanceCounter? perfomanceCounter = null)
        : ShopProductImportServiceFactory(logger, webLoaders, browserDataLoaders, itemHandler, logFactory, perfomanceCounter)
{  
    public override Type ServiceImplementationType => typeof(GoldAppleImportService);    

    protected override IImportService Create(ILogger logger, IProductShopModel shopModel, IShopImportSettings shopImportSettings, IProductItemHandler itemHandler, 
        IWebLoader webLoader, IBrowserDataLoader browserDataLoader, RequestHeaders? requestHeaders)
    {
        return new GoldAppleImportService(logger as ILogger<GoldAppleImportService>, 
            shopModel,
            webLoader,
            browserDataLoader,
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
