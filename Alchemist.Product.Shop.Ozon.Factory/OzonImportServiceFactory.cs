using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Factory.Logging;
using Alchemist.Import.Factory.Products;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Shop.Ozon.ImportService;
using Microsoft.Extensions.Logging;

namespace Alchemist.Product.Shop.Ozon.Factory;

public class OzonImportServiceFactory(ILogger<OzonImportService> logger,
   ILoaderServiceFactory loaderServiceFactory,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null)
        : ShopProductImportServiceFactory(logger, loaderServiceFactory, itemHandler, logFactory)
{
    public override Type ServiceImplementationType => typeof(OzonImportService);

    protected override IImportService Create(ILogger logger, IProductShopModel shopModel, IShopImportSettings shopImportSettings,
        IProductItemHandler itemHandler, ILoaderService browserService)
    {
        return new OzonImportService(logger as ILogger<OzonImportService>,
            shopModel,
            browserService,
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
