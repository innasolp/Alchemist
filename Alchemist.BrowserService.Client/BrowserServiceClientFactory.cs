using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client;

public class BrowserServiceClientFactory(IHttpClientFactory httpClientFactory,
    IEnumerable<IWebLoader> webLoaders,
    [FromKeyedServices(nameof(BrowserServiceClientFactory))] string apiHost) : ILoaderServiceFactory
{
    private readonly string _apiHost = apiHost;

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    private readonly IEnumerable<IWebLoader> _webLoaders = webLoaders;

    public ILoaderService Create(string name, IShopImportSettings shopImportSettings)
    {
        var webLoaderSettings = shopImportSettings.GetWebLoader<IServiceSettings>() ??
            throw new InvalidDataException($"Webloader in settings {name} not exists.");
       
         var webLoader = _webLoaders.FirstOrDefault(f => f.GetType().Name == webLoaderSettings.ImplementationTypeName
            || f.GetType().Name.Contains(webLoaderSettings.ImplementationTypeName, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new InvalidDataException($"Web loader of type {webLoaderSettings.ImplementationTypeName} not found");

        var requestHeaders = JsonSerializer.Deserialize<JsonObject>(shopImportSettings.GetRequestHeaders<IServiceSettings>().Value);

        return new BrowserServiceClient(
            _httpClientFactory,
            name,
            _apiHost,
            shopImportSettings.ShopUrl,
            webLoader,
            shopImportSettings.GetBrowserDataLoader<IServiceSettings>().ImplementationTypeName,
            shopImportSettings.GetBrowserLauncher<IServiceSettings>().ImplementationTypeName, 
            requestHeaders);
    }
}
