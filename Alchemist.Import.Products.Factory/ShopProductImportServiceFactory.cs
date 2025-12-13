using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Product;
using Import.Factory.Interfaces;
using Import.Factory.Service;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Import.Factory.Products;

internal record struct RequestData(string HttpMethod, JsonObject? Data)
{
    public RequestData() : this("GET", null) { }
}

public abstract class ShopProductImportServiceFactory(ILogger logger,
    ILoaderServiceFactory browserServiceFactory,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null) : ImportServiceFactory(logger, browserServiceFactory, logFactory)
{
    private readonly IProductItemHandler _itemHandler = itemHandler;


    protected override IImportService Create(ILogger logger, 
        string name,
        IImportSource shopModel, 
        IShopImportSettings shopImportSettings,
        ILoaderService browserService)
    {
        if (shopImportSettings is not IProductShopImportSettings productShopImportSettings)
            throw new InvalidDataException($"Invalid settings type {shopImportSettings.GetType().Name}");

        if (shopModel is not IProductShopModel productShopModel)
            throw new InvalidDataException($"Invalid shop model type {shopModel.GetType().Name}");
        
        var categoryDataService = shopImportSettings.GetService("CategoryData");
        var categoryData = categoryDataService != null && !string.IsNullOrEmpty(categoryDataService.Value)
            ? JsonSerializer.Deserialize<RequestData>(categoryDataService.Value)
            : default;

        var productDataService = shopImportSettings.GetService("ProductData");
        var productData = productDataService != null && !string.IsNullOrEmpty(productDataService.Value)
            ? JsonSerializer.Deserialize<RequestData>(productDataService.Value)
            : default;

        return Create(logger, name, productShopModel, productShopImportSettings, _itemHandler, browserService,
            productData.HttpMethod, productData.Data?.ToString(), categoryData.HttpMethod, categoryData.Data?.ToString());
    }

    protected abstract IImportService Create(ILogger logger,
        string name,
        IProductShopModel shopModel,
        IProductShopImportSettings productShopImportSettings,
        IProductItemHandler productItemHandler, 
        ILoaderService browserService,
        string? productHttpMethod,
        string? productDataFormat,
        string? categoryHttpMethod,
        string? categoryDataFormat);
}
