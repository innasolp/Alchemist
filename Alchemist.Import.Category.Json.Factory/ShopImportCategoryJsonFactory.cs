using Alchemist.Import.Factory;
using Alchemist.Import.Html;
using Alchemist.Import.Interfaces;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Category.Json.Factory;

public class ShopImportCategoryJsonFactory(ILogger<ShopImportCategoriesTimerService> logger,
    IHtmlSearcher? htmlSearcher,
    IWebLoader webLoader,
    RequestHeaders? requestHeaders,
    CategoryLoadOptions categoryLoadOptions) : IShopImportServiceFactory
{
    private readonly ILogger<ShopImportCategoriesTimerService> _logger = logger;
    private readonly IHtmlSearcher? _htmlSearcher = htmlSearcher;
    private readonly IWebLoader _webLoader = webLoader;
    private readonly RequestHeaders? _requestHeaders = requestHeaders;
    private readonly CategoryLoadOptions _categoryLoadOptions = categoryLoadOptions;

    public IImportService Create(IShopModel categoryShopModel)
    {
        return new ShopImportCategoriesTimerService(_logger, _htmlSearcher, _webLoader, categoryShopModel, _requestHeaders, _categoryLoadOptions);
    }
}
