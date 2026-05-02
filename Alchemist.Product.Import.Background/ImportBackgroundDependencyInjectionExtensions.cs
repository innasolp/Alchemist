using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Message.SignalR.HubMessage.DependencyInjection;

namespace Alchemist.Product.Import.Background;

public static class ImportBackgroundDependencyInjectionExtensions
{
    public static IServiceCollection AddShopImportDataReceiver(this IServiceCollection services, IConfiguration configuration, string signalRUrlSectionName, object? key)
    {
        var signalRUrl = configuration.GetSection(signalRUrlSectionName).Get<string>();

        return services.AddKeyedSignalRHubMessageReceiver(signalRUrl, key);
    }

    public static IServiceCollection AddSignalRMessageSender(this IServiceCollection services, IConfiguration configuration, string signalRUrlSectionName, object? key)
    {
        var signalRUrl = configuration.GetSection(signalRUrlSectionName).Get<string>();

        return services.AddKeyedSignalRHubMessageSender(signalRUrl, key);
    }    
}