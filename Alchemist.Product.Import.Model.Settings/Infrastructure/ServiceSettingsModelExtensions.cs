using Alchemist.Import.Settings.Extensions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ServiceSettingsModelExtensions
{
    public static bool IsEmpty(this IServiceSettingsModel serviceSettingsModel)
    {
        return string.IsNullOrEmpty(serviceSettingsModel.AssemblyPath) && string.IsNullOrEmpty(serviceSettingsModel.ServiceProviderPath)
            && string.IsNullOrEmpty(serviceSettingsModel.ServiceTypeName) && string.IsNullOrEmpty(serviceSettingsModel.ImplementationTypeName)
            && string.IsNullOrEmpty(serviceSettingsModel.Value);
    }
   

    public static void Update(this IServiceSettingsModel target, IServiceSettingsModel source)
    {
        target.Name = source.Name;

        target.ServiceProviderPath = source.ServiceProviderPath;
        target.ServiceTypeName = source.ServiceTypeName;
        target.ImplementationTypeName = source.ImplementationTypeName;
        target.Value = source.Value;
        target.AssemblyPath = source.AssemblyPath;
        target.FileName = source.FileName;
        target.JsonValue = source.JsonValue ?? (!string.IsNullOrEmpty(source.Value) ? JsonSerializer.Deserialize<JsonObject>(source.Value) : null);
    }

    public static void Reset(this IServiceSettingsModel serviceSettingsModel)
    {
        serviceSettingsModel.AssemblyPath = null;
        serviceSettingsModel.ServiceProviderPath = null;
        serviceSettingsModel.ServiceTypeName = null;
        serviceSettingsModel.Value = null;
        serviceSettingsModel.ImplementationTypeName = null;
        serviceSettingsModel.JsonValue = null;
        serviceSettingsModel.ImplementationTypeName = null;
    }

    internal static IServiceSettingsModel? GetPrimaryServiceSettings(this IShopImportSettingsModel shopSettings, string serviceName)
    {
        var serviceSettings = shopSettings.GetPrimaryService(serviceName);
        if(serviceSettings is IServiceSettingsModel serviceSettingsModel)
            return serviceSettingsModel;
        
        if(serviceSettings != null)
            throw new InvalidOperationException($"Invalid service type {serviceSettings?.GetType().Name}");

        return default;
    }

    internal static IServiceSettingsModel? GetServiceSettings(this IShopImportSettingsModel shopSettings, string serviceName)
    {
        return shopSettings.GetService<IServiceSettingsModel>(serviceName);
    }
}
