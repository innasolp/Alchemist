using Import.Factory.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client;

public static class BrowserServiceClientDependencyInjectionExtensions
{
    public static IServiceCollection AddBrowserServiceClientFactory(this IServiceCollection services, string apiHost)
    {
        return services.AddSingleton<ILoaderServiceFactory>((serviceProvider) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var webLoaders = serviceProvider.GetServices<IWebLoader>();
            return new BrowserServiceClientFactory(httpClientFactory, webLoaders, apiHost);
        });
    }
}
