using Alchemist.Import.Factory;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.Shop.GoldApple.ImportService;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Product.Shop.GoldApple.Factory;

public class GoldappleImportServiceFactory(IWebLoader webLoader, RequestHeaders requestHeaders, ILogger<GoldAppleImportService> logger) : IShopImportServiceFactory
{
    private readonly IWebLoader _webLoader = webLoader;

    private readonly RequestHeaders _requestHeaders = requestHeaders;

    private readonly ILogger<GoldAppleImportService> _logger = logger;

    public IImportService Create(IShopModel productShopModel)
    {
        return new GoldAppleImportService(_logger, productShopModel as IProductShopModel, _webLoader, _requestHeaders);
    }
}
