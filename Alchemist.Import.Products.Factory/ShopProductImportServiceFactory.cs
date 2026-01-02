using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Product;
using Import.Factory.Interfaces;
using Import.Factory.Service;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Factory.Products;

public abstract class ShopProductImportServiceFactory(ILogger logger,
    ILoaderServiceFactory browserServiceFactory,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null) : ImportServiceFactory(logger, browserServiceFactory, logFactory)
{
    private readonly IProductItemHandler _itemHandler = itemHandler;

    protected override IImportService Create(ILogger logger, 
        string name,
        IImportSource shopModel, 
        IImportSettings shopImportSettings,
        ILoaderService browserService)
    {
        if (shopImportSettings is not IProductShopImportSettings productShopImportSettings)
            throw new InvalidDataException($"Invalid settings type {shopImportSettings.GetType().Name}");

        if (shopModel is not IProductShopModel productShopModel)
            throw new InvalidDataException($"Invalid shop model type {shopModel.GetType().Name}");
        
        var categoryDataService = shopImportSettings.GetService("CategoryRequestOptions");
        var categoryData = categoryDataService?.GetServiceValue<ImportRequestOptions>();

        var productDataService = shopImportSettings.GetService("ProductRequestOptions");
        var productData = productDataService?.GetServiceValue<ImportRequestOptions>();

        return Create(logger, name,
            productShopModel, productShopImportSettings,
            _itemHandler, browserService,
            productData, categoryData);
    }

    protected abstract IImportService Create(ILogger logger,
        string name,
        IProductShopModel shopModel,
        IProductShopImportSettings productShopImportSettings,
        IProductItemHandler productItemHandler, 
        ILoaderService browserService,
    object? productLoadData = null,
    object? categoryLoadData = null);
}