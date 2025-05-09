using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Factory;
using Alchemist.Import.Html.Factory;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Category.Json.Factory;

public class ShopImportCategoryJsonFactory(ILogger<ShopImportCategoriesTimerService> logger, 
    IEnumerable<IWebLoaderFactory> webLoaderFactories,
    ICategoryItemHandler categoryItemHandler) 
    : IShopImportServiceFactory
{
    private readonly ILogger<ShopImportCategoriesTimerService> _logger = logger;
    private readonly IEnumerable<IWebLoaderFactory> _webLoaderFactories = webLoaderFactories;
    private readonly ICategoryItemHandler _categoryItemHandler = categoryItemHandler;

    Type IShopImportServiceFactory.ServiceImplementationType => typeof(ShopImportCategoriesTimerService);

    public IImportService Create(IShopModel shopModel, IShopImportSettings shopImportSettings)
    {
        var webLoaderFactory = _webLoaderFactories.FirstOrDefault(f => f.WebLoaderType.Name == shopImportSettings.WebLoader.ImplementationTypeName)
            ?? throw new InvalidDataException($"Web loader of type {shopImportSettings.WebLoader.ImplementationTypeName} not found");
        var webLoader = webLoaderFactory.CreateWebLoader(shopImportSettings.BrowserDataLoader?.ImplementationTypeName ?? "");
        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(shopImportSettings.RequestHeaders.Value);

        var categoryLoadOptions = JsonSerializer.Deserialize<CategoryLoadOptions>(shopImportSettings.Services.OfType<IImportServiceSettings>().FirstOrDefault(s => s.ServiceTypeName == nameof(CategoryLoadOptions))?.Value);

        var htmlSearchFactoryOptions = JsonSerializer.Deserialize<HtmlSearchFactoryOptions>(shopImportSettings.Services.OfType<IImportServiceSettings>().FirstOrDefault(s => s.ServiceTypeName == nameof(HtmlSearchFactoryOptions))?.Value);

        var htmlSearcher = HtmlSearchFactory.CreateSearcher(htmlSearchFactoryOptions?.SearchMatchType, htmlSearchFactoryOptions?.SearchElementType);

        return new ShopImportCategoriesTimerService(_logger, htmlSearcher, webLoader, shopModel, requestHeaders, categoryLoadOptions, _categoryItemHandler);
    }    
}
