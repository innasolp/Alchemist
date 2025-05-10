using Alchemist.Import.Factory;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Logging;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Shop.GoldApple.ImportService;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Product.Shop.GoldApple.Factory;

public class GoldappleImportServiceFactory(ILogger<GoldAppleImportService> logger, 
    IEnumerable<IWebLoaderFactory> webLoaderFactories,
    IProductItemHandler productItemHandler) 
    : IShopImportServiceFactory
{
    private readonly IEnumerable<IWebLoaderFactory> _webLoaderFactories = webLoaderFactories;

    private readonly ILogger<GoldAppleImportService> _logger = logger;

    private readonly IProductItemHandler _productItemHandler = productItemHandler;

    private readonly IImportServiceLogFactory? _logFactory;

    Type IShopImportServiceFactory.ServiceImplementationType => typeof(GoldAppleImportService);

    public GoldappleImportServiceFactory(ILogger<GoldAppleImportService> logger,
    IEnumerable<IWebLoaderFactory> webLoaderFactories,
    IProductItemHandler productItemHandler,
    IImportServiceLogFactory logFactory):this(logger,webLoaderFactories, productItemHandler)
    {
        _logFactory = logFactory;
    }

    public IImportService Create(IShopModel shopModel, IShopImportSettings shopImportSettings)
    {
        var webLoaderFactory = _webLoaderFactories.FirstOrDefault(f => f.WebLoaderType.Name == shopImportSettings.WebLoader.ImplementationTypeName
            || f.WebLoaderType.Name.Contains(shopImportSettings.WebLoader.ImplementationTypeName, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new InvalidDataException($"Web loader of type {shopImportSettings.WebLoader.ImplementationTypeName} not found");
        
        var webLoader = webLoaderFactory.CreateWebLoader(shopImportSettings.BrowserDataLoader?.ImplementationTypeName ?? "");
        
        var requestHeaders = shopImportSettings.RequestHeaders != null ? JsonSerializer.Deserialize<RequestHeaders>(shopImportSettings.RequestHeaders.Value) : null;

        var logger = _logFactory?.GetLogger(_logger, shopModel, shopImportSettings) ?? _logger;

        return new GoldAppleImportService(logger, shopModel as IProductShopModel, webLoader, requestHeaders, _productItemHandler);
    }
}
