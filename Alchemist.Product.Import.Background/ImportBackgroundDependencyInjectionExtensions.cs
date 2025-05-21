using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using DependencyInjection.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.DependencyInjection.Common;
using Message.SignalR.DependencyInjection;
using Grpc.Client.Extensions;

namespace Alchemist.Product.Import.Background;

public static class ImportBackgroundDependencyInjectionExtensions
{
    public static void SetAppPath(this IShopImportSettings shopImportSettings, string appPath)
    {
        shopImportSettings.BrowserDataLoader?.SetAppPath(appPath);
        shopImportSettings.WebLoader?.SetAppPath(appPath);
        shopImportSettings.ImportService.SetAppPath(appPath);
        shopImportSettings.RequestHeaders?.SetAppPath(appPath);

        foreach(var serviceSettings in shopImportSettings.Services.OfType<IImportServiceSettings>().Where(s=>!Alchemist.Import.Settings.Interfaces.Common.BaseServiceNames.Contains(s.Name)))
            serviceSettings.SetAppPath(appPath);
    }

    private static void SetAppPath(this IServiceSettings serviceSettings, string appPath)
    {
        if(!string.IsNullOrEmpty(serviceSettings.ServiceProviderPath))
            serviceSettings.ServiceProviderPath = Utils.CombinePath(appPath, serviceSettings.ServiceProviderPath);
        
        if(!string.IsNullOrEmpty(serviceSettings.AssemblyPath))
            serviceSettings.AssemblyPath = Utils.CombinePath(appPath, serviceSettings.AssemblyPath); 
    }

    public static IServiceCollection AddShopImportDataReceiver(this IServiceCollection services, IConfiguration configuration, string signalRUrlSectionName)
    {
        var signalRUrl = configuration.GetHostSectionValue(signalRUrlSectionName);

        return services.AddKeyedSignalRMessageReceiver(signalRUrl, ShopImportWorkerKeys.DataMessageReceiverKey);
    }

    public static IServiceCollection AddGrpcServiceClient<T>(this IServiceCollection services, IConfiguration configuration, string grpcApiSectionName)
        where T : class, IProductDataService
    {
        var grpcApiHost = configuration.GetSection(grpcApiSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();
        services.AddGrpcChannelWithoutCertificateCheck(grpcApiHost);
        return services.AddSingleton<IProductDataService, T>();
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

    public static IServiceCollection AddShopImportMessageSender(this IServiceCollection services, IConfiguration configuration, string signalRUrlSectionName, object? key)
    {
        var signalRUrl = configuration.GetHostSectionValue(signalRUrlSectionName);

        return services.AddKeyedSignalRMessageSender(signalRUrl, key);
    }
}
