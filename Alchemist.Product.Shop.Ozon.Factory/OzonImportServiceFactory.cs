using Alchemist.Import.Factory.Products;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Alchemist.Import.Settings.Product;
using Alchemist.Product.Shop.Ozon.ImportService;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Product.Shop.Ozon.Factory;

public class OzonImportServiceFactory(ILogger<OzonImportService> logger,
   ILoaderServiceFactory loaderServiceFactory,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null)
        : ShopProductImportServiceFactory(logger, loaderServiceFactory, itemHandler, logFactory)
{
    public override Type ServiceImplementationType => typeof(OzonImportService);

    protected override IImportService Create(ILogger logger, string name, IProductShopModel shopModel, IProductShopImportSettings productShopImportSettings,
        IProductItemHandler itemHandler, ILoaderService loaderService,
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

        return new OzonImportService(logger as ILogger<OzonImportService>,
            name,
            loaderService,
            shopModel.Url,
            shopModel.Categories,
            itemHandler,
            productShopImportSettings.ProductUrlFormat,
            productShopImportSettings.CategoryUrlFormat,
            shopModel.Name, 
            importProductServiceOptions
            );
    }

    protected override ILogger GetLogger(ILogger logger,string name, IImportServiceLogFactory importServiceLogFactory, IImportSource shopModel, IImportSettings shopImportSettings)
    {
        if (logger is ILogger<OzonImportService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, shopModel, shopImportSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}