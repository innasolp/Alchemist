using Import.Factory.Interfaces;
using Alchemist.Import.Factory.Products;
using Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Import.Settings.Interfaces;
using Alchemist.Product.Shop.GoldApple.ImportService;
using Microsoft.Extensions.Logging;
using Alchemist.Import.Settings.Product;
using Alchemist.Import.Products.Service;

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
        IProductShopImportSettings productShopImportSettings, 
        IProductItemHandler itemHandler, 
        ILoaderService loaderService,
    object? productLoadData = null,
    object? categoryLoadData = null)
    {
        var importProductServiceOptions = new ImportProductServiceOptions
        {
            PageProductCount = productShopImportSettings.PageProductCount,
            ProductLoadData = productLoadData,
            CategoryLoadData = categoryLoadData,
            ProductUrlFormatType = productShopImportSettings.ProductUrlFormatType,
            CategoryUrlFormatType = productShopImportSettings.CategoryUrlFormatType
        };

        return new GoldAppleImportService(logger as ILogger<GoldAppleImportService>,
             name,
            loaderService,
            shopModel.Url,
            shopModel.Categories,
            itemHandler,
            productShopImportSettings.ProductUrlFormat,
            productShopImportSettings.CategoryUrlFormat,
            shopModel.Name,
            importProductServiceOptions);
    }

    protected override ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IImportSource shopModel, IImportSettings shopImportSettings)
    {
        if(logger is ILogger< GoldAppleImportService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, shopModel, shopImportSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}