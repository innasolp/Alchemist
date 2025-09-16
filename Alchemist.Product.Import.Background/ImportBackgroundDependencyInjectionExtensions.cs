using Alchemist.Common;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.DependencyInjection.Common;
using Message.Interfaces;
using Alchemist.Product.Import.Background.ImportItems;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Category.Interfaces;
using Message.SignalR.HubMessage.DependencyInjection;

namespace Alchemist.Product.Import.Background;

public static class ImportBackgroundDependencyInjectionExtensions
{
    public static void SetAppPath(this IShopImportSettings shopImportSettings, string appPath)
    {
        foreach(var serviceSettings in shopImportSettings.Services.OfType<IImportServiceSettings>())
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

    public static IServiceCollection AddShopImportMessageSender(this IServiceCollection services, IConfiguration configuration, string signalRUrlSectionName, object? key)
    {
        var signalRUrl = configuration.GetHostSectionValue(signalRUrlSectionName);

        return services.AddKeyedSignalRHubMessageSender(signalRUrl, key);
    }

    public static IServiceCollection AddProductItemHandler(this IServiceCollection services, object messageSenderKey, string methodName)
    {
        return services.AddSingleton<IProductItemHandler>((serviceProvider) =>
        {
            var messageSender = serviceProvider.GetRequiredKeyedService<IMessageSender>(messageSenderKey);
            return new ProductItemHandler(messageSender, methodName);
        });
    }

    public static IServiceCollection AddCategoryItemHandler(this IServiceCollection services, object messageSenderKey, string methodName)
    {
        return services.AddSingleton<ICategoryItemHandler>((serviceProvider) =>
        {
            var messageSender = serviceProvider.GetRequiredKeyedService<IMessageSender>(messageSenderKey);
            return new CategoryItemHandler(messageSender, methodName);
        });
    }
}
