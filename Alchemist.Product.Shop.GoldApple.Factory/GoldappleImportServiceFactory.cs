using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Factory.Logging;
using Alchemist.Import.Factory.Products;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Shop.GoldApple.ImportService;
using Microsoft.Extensions.Logging;

namespace Alchemist.Product.Shop.GoldApple.Factory;

public class GoldappleImportServiceFactory(ILogger<GoldAppleImportService> logger,
    ILoaderServiceFactory browserServiceFactory,
    IProductItemHandler itemHandler, 
    IImportServiceLogFactory? logFactory = null)
        : ShopProductImportServiceFactory(logger, browserServiceFactory, itemHandler, logFactory)
{  
    public override Type ServiceImplementationType => typeof(GoldAppleImportService);    

    protected override IImportService Create(ILogger logger,
        IProductShopModel shopModel,
        IShopImportSettings shopImportSettings, 
        IProductItemHandler itemHandler, 
        ILoaderService loaderService)
    {
        return new GoldAppleImportService(logger as ILogger<GoldAppleImportService>, 
            shopModel,           
            loaderService,             
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
