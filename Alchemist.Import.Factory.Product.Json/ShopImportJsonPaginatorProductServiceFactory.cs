using Alchemist.Import.Factory.Products;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Product;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Factory.Product.Json;

internal class ShopImportJsonPaginatorProductServiceFactory(ILogger<ShopImportHttpJsonPagingCategoryProductsService> logger, 
    ILoaderServiceFactory browserServiceFactory,
    IEnumerable<IProductItemHandlerFactory> itemHandlerFactories,
    IImportServiceLogFactory? logFactory = null) 
    : ShopProductImportServiceFactory(logger, browserServiceFactory, itemHandlerFactories, logFactory)
{
    public override Type ServiceImplementationType => typeof(ShopImportHttpJsonPagingCategoryProductsService);

    protected override IImportService Create(ILogger logger, string name, IProductShopSource shopModel, IProductShopImportSettings productShopImportSettings, IProductItemHandler productItemHandler, ILoaderService browserService, object? productLoadData = null, object? categoryLoadData = null)
    {
        var categoryJsonSettingsService = productShopImportSettings.GetService("CategoryJsonSettings");
        var categoryJsonSettings = categoryJsonSettingsService.GetServiceValue<JsonSettings>();

        var importProductJsonServiceOptions = new ImportProductJsonServiceOptions
        {
            CategoryJsonSettings = categoryJsonSettings,
            PageProductCount = productShopImportSettings.PageProductCount,
            ProductLoadData = productLoadData,
            CategoryLoadData = categoryLoadData,
            ProductPathFormatType = productShopImportSettings.ProductUrlFormatType,
            CategoryPathFormatType = productShopImportSettings.CategoryUrlFormatType
        };

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