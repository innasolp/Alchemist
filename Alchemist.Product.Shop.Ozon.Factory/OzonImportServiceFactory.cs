using Alchemist.Import.Factory;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.Shop.Ozon.ImportService;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Product.Shop.Ozon.Factory;

public class OzonImportServiceFactory(IWebLoader webLoader, RequestHeaders requestHeaders, ILogger<OzonImportService> logger) : IShopImportServiceFactory
{
    private readonly IWebLoader _webLoader = webLoader;

    private readonly RequestHeaders _requestHeaders = requestHeaders;

    private readonly ILogger<OzonImportService> _logger = logger;

    public IImportService Create(IShopModel productShopModel)
    {
        return new OzonImportService(_logger, productShopModel as IProductShopModel, _webLoader, _requestHeaders);
    }
}
