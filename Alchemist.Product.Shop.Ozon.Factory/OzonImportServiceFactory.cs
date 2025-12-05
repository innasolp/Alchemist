using Import.Factory.Interfaces;
using Alchemist.Import.Factory.Products;
using Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Import.Settings.Interfaces;
using Alchemist.Product.Shop.Ozon.ImportService;
using Microsoft.Extensions.Logging;
using Alchemist.Import.Settings.Product;

namespace Alchemist.Product.Shop.Ozon.Factory;

public class OzonImportServiceFactory(ILogger<OzonImportService> logger,
   ILoaderServiceFactory loaderServiceFactory,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null)
        : ShopProductImportServiceFactory(logger, loaderServiceFactory, itemHandler, logFactory)
{
    public override Type ServiceImplementationType => typeof(OzonImportService);

    protected override IImportService Create(ILogger logger, string name, IProductShopModel shopModel, IShopImportSettings shopImportSettings,
        IProductItemHandler itemHandler, ILoaderService browserService)
    {
        return new OzonImportService(logger as ILogger<OzonImportService>,
            name,
           shopModel.ProductUrl,
            shopModel.CategoryUrl,
            shopModel.Name,
            shopModel.Url,
            shopModel.Categories,
            browserService,
            itemHandler);
    }

    protected override ILogger GetLogger(ILogger logger,string name, IImportServiceLogFactory importServiceLogFactory, IImportSource shopModel, IShopImportSettings shopImportSettings)
    {
        if (logger is ILogger<OzonImportService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, shopModel, shopImportSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}