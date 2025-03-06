using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.DependencyInjection.Common;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddKeyedRestApiClient<TService, TImplementation>(this IHostApplicationBuilder builder, string restApiSectionName, string key, out IHttpClientBuilder httpClientBuilder)
        where TService:class
        where TImplementation : class, TService
    {
        var restApiHost = builder.GetHostSectionValue(restApiSectionName);
        httpClientBuilder = builder.Services.AddHttpClient(restApiHost);

        builder.Services.AddKeyedSingleton(key, restApiHost);
        return builder.Services.AddSingleton<TService, TImplementation>();
    }

    public static IServiceCollection AddRestApiClient<TService, TImplementation>(this IHostApplicationBuilder builder, string restApiSectionName, string key)
        where TService : class
        where TImplementation : class, TService
    {
        var restApiHost = builder.GetHostSectionValue(restApiSectionName);
        builder.Services.AddHttpClient();

        builder.Services.AddKeyedSingleton(key, restApiHost);
        return builder.Services.AddSingleton<TService, TImplementation>();
    }

    public static IServiceCollection AddHttpMessageDelegatingHandler<TMessageHandler>(this IHostApplicationBuilder builder, IHttpClientBuilder httpClientBuilder, string apiHost)
        where TMessageHandler : DelegatingHandler
    {
        builder.Services.AddSingleton<TMessageHandler>();

        httpClientBuilder.AddHttpMessageHandler(serviceProvider => serviceProvider.GetRequiredService<TMessageHandler>());

        return builder.Services;
    }
}
