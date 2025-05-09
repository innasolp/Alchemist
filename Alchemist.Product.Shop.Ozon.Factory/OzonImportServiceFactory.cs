using Alchemist.Import.Factory;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Shop.Ozon.ImportService;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Product.Shop.Ozon.Factory;

public class OzonImportServiceFactory(ILogger<OzonImportService> logger, 
    IEnumerable<IWebLoaderFactory> webLoaderFactories,
    IProductItemHandler productItemHandler)
    : IShopImportServiceFactory
{
    private readonly IEnumerable<IWebLoaderFactory> _webLoaderFactories = webLoaderFactories;

    private readonly ILogger<OzonImportService> _logger = logger;

    private readonly IProductItemHandler _productItemHandler = productItemHandler;

    Type IShopImportServiceFactory.ServiceImplementationType => typeof(OzonImportService);

    public IImportService Create(IShopModel shopModel, IShopImportSettings shopImportSettings)
    {
        var webLoaderFactory = _webLoaderFactories.FirstOrDefault(f => f.WebLoaderType.Name == shopImportSettings.WebLoader.ImplementationTypeName)
            ?? throw new InvalidDataException($"Web loader of type {shopImportSettings.WebLoader.ImplementationTypeName} not found");
        
        var webLoader = webLoaderFactory.CreateWebLoader(shopImportSettings.BrowserDataLoader?.ImplementationTypeName ?? "");
        
        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(shopImportSettings.RequestHeaders.Value);
        
        return new OzonImportService(_logger, shopModel as IProductShopModel, webLoader, requestHeaders, _productItemHandler);
    }
}
