using Alchemist.Import.Settings;
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
    private readonly IEnumerable<IWebLoaderFactory> _webLoaderFactories;

    private readonly HttpClient _httpClient;

    public BrowserServiceClientFactory(IHttpClientFactory httpClientFactory,
        IEnumerable<IWebLoaderFactory> webLoaderFactories,
        [FromKeyedServices(nameof(BrowserServiceClientFactory))] string apiHost)
    {
        _webLoaderFactories = webLoaderFactories;

        _httpClient = httpClientFactory.CreateClient(apiHost);
        _httpClient.BaseAddress = new Uri(apiHost);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public ILoaderService Create(string name, IImportSettings importSettings)
    {
        if(importSettings is not IShopImportSettings shopImportSettings)
            throw new InvalidDataException($"invalid settings type {importSettings.GetType().Name}.");

        var webLoaderSettings = shopImportSettings.GetWebLoader<IServiceSettings>() ??
            throw new InvalidDataException($"Webloader in settings {name} not exists.");
       
         var webLoaderFactory = _webLoaderFactories.FirstOrDefault(f => f.GetType().Name == webLoaderSettings.ImplementationTypeName
            || f.GetType().Name.Contains(webLoaderSettings.ImplementationTypeName, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new InvalidDataException($"Web loader factory for type {webLoaderSettings.ImplementationTypeName} not found");

        var requestHeadersService = shopImportSettings.GetRequestHeaders<IServiceSettings>();
        var requestHeaders = requestHeadersService != null ? JsonSerializer.Deserialize<RequestHeaders>(requestHeadersService.Value) : null ;

        return new BrowserServiceClient(
            _httpClient,
            name,
            shopImportSettings.ShopUrl,
            webLoaderFactory.CreateWebLoader(),
            shopImportSettings.GetBrowserDataLoader<IServiceSettings>()?.ImplementationTypeName,
            shopImportSettings.GetBrowserLauncher<IServiceSettings>()?.ImplementationTypeName, 
            requestHeaders);
    }
}