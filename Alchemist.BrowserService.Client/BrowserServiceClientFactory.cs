using Alchemist.Import.Settings.Extensions;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text.Json;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client;

public class BrowserServiceClientFactory : ILoaderServiceFactory
{
    private readonly IEnumerable<IWebLoader> _webLoaders;

    private readonly HttpClient _httpClient;

    public BrowserServiceClientFactory(IHttpClientFactory httpClientFactory,
        IEnumerable<IWebLoader> webLoaders,
        [FromKeyedServices(nameof(BrowserServiceClientFactory))] string apiHost)
    {
        _webLoaders = webLoaders;

        _httpClient = httpClientFactory.CreateClient(apiHost);
        _httpClient.BaseAddress = new Uri(apiHost);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public ILoaderService Create(string name, IShopImportSettings shopImportSettings)
    {
        var webLoaderSettings = shopImportSettings.GetWebLoader<IServiceSettings>() ??
            throw new InvalidDataException($"Webloader in settings {name} not exists.");
       
         var webLoader = _webLoaders.FirstOrDefault(f => f.GetType().Name == webLoaderSettings.ImplementationTypeName
            || f.GetType().Name.Contains(webLoaderSettings.ImplementationTypeName, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new InvalidDataException($"Web loader of type {webLoaderSettings.ImplementationTypeName} not found");

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(shopImportSettings.GetRequestHeaders<IServiceSettings>().Value);

        return new BrowserServiceClient(
            _httpClient,
            name,            
            shopImportSettings.ShopUrl,
            webLoader,
            shopImportSettings.GetBrowserDataLoader<IServiceSettings>().ImplementationTypeName,
            shopImportSettings.GetBrowserLauncher<IServiceSettings>().ImplementationTypeName, 
            requestHeaders);
    }
}
