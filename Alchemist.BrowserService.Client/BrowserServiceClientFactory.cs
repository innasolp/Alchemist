using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.BrowserService.Client;

public class BrowserServiceClientFactory(IHttpClientFactory httpClientFactory, 
    [FromKeyedServices(nameof(BrowserServiceClientFactory))] string apiHost) : IBrowserServiceFactory
{
    private readonly string _apiHost = apiHost;

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public IBrowserService Create(string browserLoader, string browserLauncher)
    {
        return new BrowserServiceClient(_httpClientFactory, _apiHost, browserLoader, browserLauncher);
    }
}
