using Alchemist.Import.Factory.Product.Json.Infrastructure;
using Alchemist.Import.Factory.Products;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Product;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Factory.Product.Json;

public abstract class ShopImportJsonProductServiceFactory(ILogger logger,
    ILoaderServiceFactory browserServiceFactory,
    IEnumerable<IProductItemHandlerFactory> itemHandlerFactories,
    IImportServiceLogFactory? logFactory = null)
    : ShopProductImportServiceFactory(logger, browserServiceFactory, itemHandlerFactories, logFactory)
{
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

        return CreateWithJsonServiceOptions(logger, name, shopModel, productShopImportSettings, productItemHandler, browserService, importProductJsonServiceOptions);
    }

    protected abstract IImportService CreateWithJsonServiceOptions(ILogger logger,
        string name,
        IProductShopSource shopModel,
        IProductShopImportSettings productShopImportSettings,
        IProductItemHandler productItemHandler,
        ILoaderService browserService,
        ImportProductJsonServiceOptions importProductJsonServiceOptions);
}