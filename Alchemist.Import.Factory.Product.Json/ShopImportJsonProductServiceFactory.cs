using Alchemist.Import.Factory.Products;
using Alchemist.Import.Product.Json.Service;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Json;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Product;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Factory.Product.Json;

public class ShopImportJsonProductServiceFactory(ILogger<ShopImportJsonCategoryProductsService> logger, ILoaderServiceFactory browserServiceFactory, IProductItemHandler itemHandler, IImportServiceLogFactory? logFactory = null) 
    : ShopProductImportServiceFactory(logger, browserServiceFactory, itemHandler, logFactory)
{
    public override Type ServiceImplementationType => typeof(ShopImportJsonCategoryProductsService);

    protected override IImportService Create(ILogger logger, string name, IProductShopModel shopModel, IProductShopImportSettings productShopImportSettings, IProductItemHandler productItemHandler, ILoaderService browserService, object? productLoadData = null, object? categoryLoadData = null)
    {
        var productJsonSettingsService = productShopImportSettings.GetService("ProductJsonSettings");
        var productJsonSettings = productJsonSettingsService.GetServiceValue<JsonSettings>();

        var categoryJsonSettingsService = productShopImportSettings.GetService("CategoryJsonSettings");
        var categoryJsonSettings = categoryJsonSettingsService.GetServiceValue<JsonSettings>();

        return new ShopImportJsonCategoryProductsService(logger as ILogger<ShopImportJsonCategoryProductsService>,
            browserService,
            shopModel.Url,
            //todo
            $"{shopModel.Name}_product",
            shopModel.Categories,
            productItemHandler,
            productShopImportSettings.ProductUrlFormat,
            productShopImportSettings.CategoryUrlFormat,
            shopModel.Name,
            productJsonSettings,
            categoryJsonSettings,
            productShopImportSettings.PageProductCount,
             productLoadData,
            categoryLoadData,
            productShopImportSettings.ProductUrlFormatType,
            productShopImportSettings.CategoryUrlFormatType);
    }

    protected override ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IImportSource importSource, IImportSettings importSettings)
    {
        if (logger is ILogger<ShopImportJsonCategoryProductsService<CategoryProducts, Import.Products.Json.Product>> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, importSource, importSettings) ?? serviceLogger;
        else
            throw new InvalidDataException(logger.GetType().FullName);
    }
}