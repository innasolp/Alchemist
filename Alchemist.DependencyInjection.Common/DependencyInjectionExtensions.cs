using Alchemist.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.DependencyInjection.Common;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRestApiClient<TService, TImplementation>(this IServiceCollection services, IConfiguration configuration, string restApiSectionName, string key, out IHttpClientBuilder httpClientBuilder)
        where TService:class
        where TImplementation : class, TService
    {
        var restApiHost = configuration.GetHostSectionValue(restApiSectionName);
        httpClientBuilder = services.AddHttpClient(restApiHost);

        services.AddKeyedSingleton(key, restApiHost);
        return services.AddSingleton<TService, TImplementation>();
    }    
    
    public static IServiceCollection AddRestApiClient<TService, TImplementation>(this IServiceCollection services, IConfiguration configuration, string restApiSectionName, string key)
        where TService : class
        where TImplementation : class, TService
    {
        var restApiHost = configuration.GetSection(restApiSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();
        services.AddHttpClient();

        services.AddKeyedSingleton(key, restApiHost);
        return services.AddSingleton<TService, TImplementation>();
    }

    public static IServiceCollection SetHttpMessageDelegatingHandler<TMessageHandler>(this IServiceCollection services, IHttpClientBuilder httpClientBuilder, string key)
        where TMessageHandler : DelegatingHandler
    {
        //services.AddKeyedSingleton<TMessageHandler>(key);

        httpClientBuilder.AddHttpMessageHandler(serviceProvider => serviceProvider.GetRequiredKeyedService<TMessageHandler>(key));

        return services;
    }

    public static IServiceCollection SetHttpMessageDelegatingHandler<TMessageHandler>(this IServiceCollection services, IHttpClientBuilder httpClientBuilder, TMessageHandler messageHandler)
        where TMessageHandler : DelegatingHandler
    {
        //services.AddKeyedSingleton<TMessageHandler>(key);

        httpClientBuilder.AddHttpMessageHandler(serviceProvider => messageHandler);

        return services;
    }

    public static IServiceCollection AddHttpMessageDelegatingHandler<TMessageHandler>(this IServiceCollection services, IHttpClientBuilder httpClientBuilder)
        where TMessageHandler : DelegatingHandler, new()
    {
        var messageHandler = new TMessageHandler();
        
        services.AddSingleton(messageHandler);

        httpClientBuilder.AddHttpMessageHandler(serviceProvider => messageHandler);

        return services;
    }
}
