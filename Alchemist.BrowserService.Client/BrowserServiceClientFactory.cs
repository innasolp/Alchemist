using Alchemist.Import.Settings.Extensions;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.LoaderSettings;
using Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Alchemist.BrowserService.Client;

public class BrowserServiceClientFactory : ILoaderServiceFactory
{
    private readonly HttpClient _httpClient;

    private readonly IRateLimiterWebLoaderHostFactory _webLoaderHostFactory;

    public BrowserServiceClientFactory(IHttpClientFactory httpClientFactory,
        IRateLimiterWebLoaderHostFactory webLoaderHostFactory,
        [FromKeyedServices(nameof(BrowserServiceClientFactory))] string apiHost)
    {
        _webLoaderHostFactory = webLoaderHostFactory;
        _httpClient = httpClientFactory.CreateClient(apiHost);
        _httpClient.BaseAddress = new Uri(apiHost);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public ILoaderService Create(string name, IImportSettings importSettings)
    {
        if (importSettings is not IHostSettings hostSettings)
            throw new InvalidDataException($"invalid settings type {importSettings.GetType().Name}.");

        var webLoaderSettings = importSettings.GetWebLoader<IServiceSettings>() ??
            throw new InvalidDataException($"Webloader in settings {name} not exists.");
       
        var requestHeadersService = importSettings.GetRequestHeaders<IServiceSettings>();
        var requestHeaders = requestHeadersService?.GetServiceValue<RequestHeaders>();

        var hostRequestOptions = importSettings.GetService("HostRequestOptions")?.GetServiceValue<RequestOptions>();

        var rateLimiterOptions = importSettings.GetService("RateLimiterOptions")?.GetServiceValue<RateLimiterOptions>();
        var webLoader = _webLoaderHostFactory.GetRateLimiterWebLoader(webLoaderSettings, hostSettings.Host, rateLimiterOptions);
        
        return new BrowserServiceClient(
            _httpClient,
            name,
            hostSettings.Host,
            webLoader,
            hostRequestOptions,
            importSettings.GetBrowserDataLoader<IServiceSettings>()?.ImplementationTypeName,
            importSettings.GetBrowserLauncher<IServiceSettings>()?.ImplementationTypeName,
            requestHeaders);
    }
}