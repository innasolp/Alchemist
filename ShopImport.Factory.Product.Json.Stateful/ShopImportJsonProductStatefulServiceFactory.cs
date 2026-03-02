using Alchemist.Import.Factory.Product.Json;
using Alchemist.Import.Factory.Product.Json.Infrastructure;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Product;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.KeyHash;
using ShopImport.ServiceState;

namespace ShopImport.Factory.Product.Json.Stateful;

internal class ShopImportJsonProductStatefulServiceFactory(ILogger<ShopImportHttpJsonCategoryProductsStatefulService> logger,
    ILoaderServiceFactory browserServiceFactory,
    IEnumerable<IProductItemHandlerFactory> itemHandlerFactories,
    IEnumerable<IKeyHasher> keyHashers,
    IEnumerable<IServiceStateRepositoryFactory> serviceStateRepositoryFactories,
    IImportServiceLogFactory? logFactory = null) 
    : ShopImportJsonProductServiceFactory(logger, browserServiceFactory, itemHandlerFactories, logFactory)
{
    public override Type ServiceImplementationType => typeof(ShopImportHttpJsonCategoryProductsStatefulService);

    protected override IImportService CreateWithJsonServiceOptions(ILogger logger, string name, IProductShopSource shopModel, IProductShopImportSettings productShopImportSettings, IProductItemHandler productItemHandler, ILoaderService browserService, ImportProductJsonServiceOptions importProductJsonServiceOptions)
    {
        var keyHasher = keyHashers.GetKeyHasher(productShopImportSettings);

        var serviceStateRepository = serviceStateRepositoryFactories.GetServiceStateRepository(productShopImportSettings);

        return new ShopImportHttpJsonCategoryProductsStatefulService(logger as ILogger<ShopImportHttpJsonCategoryProductsStatefulService>,
            browserService,
            shopModel.Url,
            //todo
            $"{shopModel.Name}_product",
            shopModel.Categories,
            productItemHandler,
            productShopImportSettings.ProductUrlFormat,
            productShopImportSettings.CategoryUrlFormat,
            shopModel.Name,
            importProductJsonServiceOptions,
            keyHasher,
            serviceStateRepository
            );
    }    

    protected override ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IImportSource importSource, IImportSettings importSettings)
    {
        if (logger is ILogger<ShopImportHttpJsonCategoryProductsStatefulService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, importSource, importSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}