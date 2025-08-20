using Alchemist.Import.BrowserService.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.BrowserService.Client;

public static class BrowserServiceClientDependencyInjectionExtensions
{
    public static IServiceCollection AddBrowserServiceClientFactory(this IServiceCollection services, string apiHost)
    {
        return services.AddSingleton<IBrowserServiceFactory>((serviceProvider) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            return new BrowserServiceClientFactory(httpClientFactory, apiHost);
        });
    }
}
