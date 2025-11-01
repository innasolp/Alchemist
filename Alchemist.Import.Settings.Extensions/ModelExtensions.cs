using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using System.Text.Json;

namespace Alchemist.Import.Settings.Extensions;

public static class ModelExtensions
{    
    public static void Update<TService>(this TService serviceSettings, TService source)
        where TService : class, IServiceSettings
    {
        if (source == null) return;
        
        serviceSettings.ServiceProviderPath = source.ServiceProviderPath;
        serviceSettings.AssemblyPath = source.AssemblyPath;
        serviceSettings.ServiceTypeName = source.ServiceTypeName;
        serviceSettings.ImplementationTypeName = source.ImplementationTypeName;
        serviceSettings.Value = source.Value;
    }

    public static TShopImportSettings GetShopImportSettings<TShopImportSettings, TImportServiceSettings>(this IShopSettings shopSettings,
        IEnumerable<IShopSettings> services)
        where TShopImportSettings : IShopImportSettings
        where TImportServiceSettings : class, IServiceSettings
    {
        var shopImportSettings = JsonSerializer.Deserialize<TShopImportSettings>(shopSettings.JsonValue);
                
        foreach (var service in services)
        {
            var serviceSource = service.ToImportServiceSettings<TImportServiceSettings>();

            shopImportSettings.UpdateServices(service.Name, serviceSource);
        }

        return shopImportSettings;
    }

    public static void UpdateServices<TService>(this IShopImportSettings shopImportSettings, string serviceName, TService serviceSource)
        where TService : class, IServiceSettings 
    {
        var targetService = shopImportSettings.GetService<TService>(serviceName);

        if (targetService != null)
            targetService.Update(serviceSource);
        else
            shopImportSettings.Services.Add(serviceName, serviceSource);
    }

}
