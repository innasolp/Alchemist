using Import.Factory.Interfaces;
using Alchemist.Import.Factory.Products;
using Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Import.Settings.Interfaces;
using Alchemist.Product.Shop.GoldApple.ImportService;
using Microsoft.Extensions.Logging;
using Alchemist.Import.Settings.Product;

namespace Alchemist.Product.Shop.GoldApple.Factory;

public class GoldappleImportServiceFactory(ILogger<GoldAppleImportService> logger,
    ILoaderServiceFactory browserServiceFactory,
    IProductItemHandler itemHandler, 
    IImportServiceLogFactory? logFactory = null)
        : ShopProductImportServiceFactory(logger, browserServiceFactory, itemHandler, logFactory)
{  
    public override Type ServiceImplementationType => typeof(GoldAppleImportService);    

    protected override IImportService Create(ILogger logger,
        string name,
        IProductShopModel shopModel,
        IShopImportSettings shopImportSettings, 
        IProductItemHandler itemHandler, 
        ILoaderService loaderService)
    {
        return new GoldAppleImportService(logger as ILogger<GoldAppleImportService>, 
            name,
            shopModel.ProductUrl,           
            shopModel.CategoryUrl,           
            shopModel.Name,           
            shopModel.Url,           
            shopModel.Categories,           
            loaderService,             
            itemHandler);
    }

    protected override ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IImportSource shopModel, IShopImportSettings shopImportSettings)
    {
        if(logger is ILogger< GoldAppleImportService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, shopModel, shopImportSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}
