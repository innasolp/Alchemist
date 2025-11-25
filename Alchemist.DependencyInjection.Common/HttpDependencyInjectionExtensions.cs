using Alchemist.Common;
using Grpc.Client.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.DependencyInjection.Common;

public static class HttpDependencyInjectionExtensions
{
    public static IServiceCollection AddRestApiClient<TService, TImplementation>(this IServiceCollection services, IConfiguration configuration, string restApiSectionName, string hostKey, out IHttpClientBuilder httpClientBuilder)
        where TService:class
        where TImplementation : class, TService
    {
        var restApiHost = configuration.GetSection(restApiSectionName).Get<string>();
        httpClientBuilder = services.AddHttpClient(hostKey);

        services.AddKeyedSingleton(hostKey, restApiHost);
        return services.AddSingleton<TService, TImplementation>();
    }    
    
    public static IServiceCollection AddRestApiClient<TService, TImplementation>(this IServiceCollection services, IConfiguration configuration, string restApiSectionName, string hostKey)
        where TService : class
        where TImplementation : class, TService
    {
        var restApiHost = configuration.GetSection(restApiSectionName).Get<string>();
        services.AddHttpClient();

        services.AddKeyedSingleton(hostKey, restApiHost);
        return services.AddSingleton<TService, TImplementation>();
    }

    public static IServiceCollection SetHttpMessageDelegatingHandler<TMessageHandler>(this IServiceCollection services, IHttpClientBuilder httpClientBuilder, string messageHandlerKey)
        where TMessageHandler : DelegatingHandler
    {
        //services.AddKeyedSingleton<TMessageHandler>(key);

        httpClientBuilder.AddHttpMessageHandler(serviceProvider => serviceProvider.GetRequiredKeyedService<TMessageHandler>(messageHandlerKey));

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

    public static IServiceCollection AddGrpcServiceClient<TService, TImplementation>(this IServiceCollection services, IConfiguration configuration, string grpcApiSectionName)
        where TService : class
        where TImplementation : class, TService
    {
        var grpcApiHost = configuration.GetSection(grpcApiSectionName).Get<string>();
        services.AddGrpcChannelWithoutCertificateCheck(grpcApiHost);
        return services.AddSingleton<TService, TImplementation>();
    }

    public static IServiceCollection ConfigureDefaultHttps(this IServiceCollection services)
    {
        return services.ConfigureHttpClientDefaults(builder =>
        {
            builder.ConfigurePrimaryHttpMessageHandler(
                () => new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) =>
                    {
                        return true;
                    }
                });
        });
    }
}
