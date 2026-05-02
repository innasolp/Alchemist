using Import.Factory.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.BrowserService.Client;

public static class BrowserServiceClientDependencyInjectionExtensions
{
    public static IServiceCollection AddBrowserServiceClientFactory(this IServiceCollection services, string apiHost)
    {
        if (!services.OfType<ServiceDescriptor>().Any(sd => sd.ServiceType == typeof(IRateLimiterWebLoaderHostFactory)))
            services.AddSingleton<IRateLimiterWebLoaderHostFactory, RateLimiterWebLoaderHostFactory>();

        return services.AddSingleton<ILoaderServiceFactory>((serviceProvider) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var webloaderHostFactory = serviceProvider.GetRequiredService<IRateLimiterWebLoaderHostFactory>();
            return new BrowserServiceClientFactory(httpClientFactory, webloaderHostFactory, apiHost);
        });
    }
}