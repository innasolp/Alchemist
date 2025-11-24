using Alchemist.Common;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.DependencyInjection.Common;
using Message.SignalR.HubMessage.DependencyInjection;

namespace Alchemist.Product.Import.Background;

public static class ImportBackgroundDependencyInjectionExtensions
{
    public static void SetAppPath(this IShopImportSettings shopImportSettings, string appPath)
    {
        foreach(var serviceSettings in shopImportSettings.Services.OfType<IServiceSettings>())
            serviceSettings.SetAppPath(appPath);
    }

    private static void SetAppPath(this IServiceSettings serviceSettings, string appPath)
    {
        if(!string.IsNullOrEmpty(serviceSettings.ServiceProviderPath))
            serviceSettings.ServiceProviderPath = Utils.CombinePath(appPath, serviceSettings.ServiceProviderPath);
        
        if(!string.IsNullOrEmpty(serviceSettings.AssemblyPath))
            serviceSettings.AssemblyPath = Utils.CombinePath(appPath, serviceSettings.AssemblyPath); 
    }

    public static IServiceCollection AddShopImportDataReceiver(this IServiceCollection services, IConfiguration configuration, string signalRUrlSectionName, object? key)
    {
        var signalRUrl = configuration.GetHostSectionValue(signalRUrlSectionName);

        return services.AddKeyedSignalRHubMessageReceiver(signalRUrl, key);
    }

    public static IServiceCollection AddSignalRMessageSender(this IServiceCollection services, IConfiguration configuration, string signalRUrlSectionName, object? key)
    {
        var signalRUrl = configuration.GetHostSectionValue(signalRUrlSectionName);

        return services.AddKeyedSignalRHubMessageSender(signalRUrl, key);
    }    
}
