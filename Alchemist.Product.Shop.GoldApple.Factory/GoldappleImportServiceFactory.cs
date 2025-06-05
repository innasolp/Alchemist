using Alchemist.Import.Factory.Abstractions;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Logging;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Shop.GoldApple.ImportService;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Product.Shop.GoldApple.Factory;

public class GoldappleImportServiceFactory(ILogger<GoldAppleImportService> logger,
    IEnumerable<IWebLoaderFactory> webLoaderFactories,
    IProductItemHandler itemHandler, 
    IImportServiceLogFactory? logFactory = null, 
    IPerfomanceCounter? perfomanceCounter = null)
        : ShopImportFactory(logger, webLoaderFactories, itemHandler, logFactory, perfomanceCounter)
{  
    public override Type ServiceImplementationType => typeof(GoldAppleImportService);    

    protected override IImportService Create(ILogger logger, IShopModel shopModel, IShopImportSettings shopImportSettings, IWebLoader webLoader, IItemHandler itemHandler, RequestHeaders? requestHeaders)
    {
        return new GoldAppleImportService(logger as ILogger<GoldAppleImportService>, 
            shopModel as IProductShopModel,
            webLoader,
            requestHeaders, 
            itemHandler as IProductItemHandler);
    }

    protected override ILogger GetLogger(ILogger logger, IImportServiceLogFactory importServiceLogFactory, IShopModel shopModel, IShopImportSettings shopImportSettings)
    {
        if(logger is ILogger< GoldAppleImportService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, shopModel, shopImportSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}
