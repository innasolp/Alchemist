using Alchemist.Import.Factory.Product.Json.Infrastructure;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Product;
using DependencyInjection.Attributes;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Factory.Product.Json;

[DILoad]
internal class ShopImportJsonPaginatorProductServiceFactory(ILogger<ShopImportHttpJsonPagingCategoryProductsService> logger, 
    ILoaderServiceFactory browserServiceFactory,
    IEnumerable<IProductItemHandlerFactory> itemHandlerFactories,
    IImportServiceLogFactory? logFactory = null) 
    : ShopImportJsonProductServiceFactory(logger, browserServiceFactory, itemHandlerFactories, logFactory)
{
    public override Type ServiceImplementationType => typeof(ShopImportHttpJsonPagingCategoryProductsService);

    protected override IImportService CreateWithJsonServiceOptions(ILogger logger, string name, IProductShopSource shopModel, IProductShopImportSettings productShopImportSettings, IProductItemHandler productItemHandler, ILoaderService browserService, ImportProductJsonServiceOptions importProductJsonServiceOptions)
    {
        return new ShopImportHttpJsonPagingCategoryProductsService(logger as ILogger<ShopImportHttpJsonPagingCategoryProductsService>,
            browserService,
            shopModel.Url,
            //todo
            $"{shopModel.Name}_product",
            shopModel.Categories,
            productItemHandler,
            productShopImportSettings.ProductUrlFormat,
            productShopImportSettings.CategoryUrlFormat,
            shopModel.Name,
            importProductJsonServiceOptions);
    }    

    protected override ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IImportSource importSource, IImportSettings importSettings)
    {
        if (logger is ILogger<ShopImportHttpJsonPagingCategoryProductsService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, importSource, importSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}