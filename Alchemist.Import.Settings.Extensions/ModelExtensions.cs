using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using System.Text.Json;

namespace Alchemist.Import.Settings.Extensions;

public static class ModelExtensions
{  
    public static T ConvertToImportServiceSettings<T>(this IShopSettings shopSettings)
        where T : IImportServiceSettings
    {
        var serviceSettings = JsonSerializer.Deserialize<T>(shopSettings.JsonValue);
        serviceSettings.Id = shopSettings.Id;
        serviceSettings.ParentSettingsId = shopSettings.ParentSettingsId;
        serviceSettings.Name = shopSettings.Name;
        return serviceSettings;
    }    

    public static void Update(this IImportServiceSettings serviceSettings, IImportServiceSettings source)
    {
        if (source == null) return;

        if (source.Id != null && source.Id != 0) serviceSettings.Id = source.Id;
        if (source.ParentSettingsId != null && source.ParentSettingsId != 0) serviceSettings.ParentSettingsId = source.ParentSettingsId;

        serviceSettings.ServiceProviderPath = source.ServiceProviderPath;
        serviceSettings.AssemblyPath = source.AssemblyPath;
        serviceSettings.ServiceTypeName = source.ServiceTypeName;
        serviceSettings.Name = source.Name;
        serviceSettings.ImplementationTypeName = source.ImplementationTypeName;
        serviceSettings.Value = source.Value;
    }

    public static async  Task<TShopImportSettings> GetShopImportSettings<TShopImportSettings, TImportServiceSettings>(this IShopSettings shopSettings,
        IEnumerable<IShopSettings> services)
        where TShopImportSettings : IShopImportSettings
        where TImportServiceSettings : IImportServiceSettings
    {
        var shopImportSettings = JsonSerializer.Deserialize<TShopImportSettings>(shopSettings.JsonValue);

        shopImportSettings.Id = shopSettings.Id;
        shopImportSettings.ShopId = shopSettings.ShopId;

        foreach (var service in services)
        {
            var serviceSource = service.ConvertToImportServiceSettings<TImportServiceSettings>();

            shopImportSettings.UpdateServiceSettings(serviceSource.Name, serviceSource);
        }

        return shopImportSettings;
    }

    public static void UpdateServiceSettings(this IShopImportSettings shopImportSettings, string serviceName, IImportServiceSettings serviceSource)       
    {
        var targetService = shopImportSettings.GetService(serviceName);

        if (targetService != null)
            targetService.Update(serviceSource);
        else
            shopImportSettings.Services.Add(serviceSource);
    }

}
