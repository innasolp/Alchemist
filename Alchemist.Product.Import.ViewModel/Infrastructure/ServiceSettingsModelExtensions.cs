using Alchemist.Import.Settings.Interfaces;
using DependencyInjection.Interfaces;
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
        if (!ModelHelper.IsServiceSettingsPrimary(source.Name))
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

    internal static IServiceSettingsModel? GetServiceSettings(this IShopServicesSettingsModel shopSettings, string serviceName)
    {
        return serviceName switch
        {
            nameof(IShopServicesSettingsModel.ImportService) => shopSettings.ImportService,
            nameof(IShopServicesSettingsModel.WebLoader) => shopSettings.WebLoader,
            nameof(IShopServicesSettingsModel.BrowserDataLoader) => shopSettings.BrowserDataLoader,
            nameof(IShopServicesSettingsModel.RequestHeaders) => shopSettings.RequestHeaders,
            _ => null,
        };
    }

    public static bool IsEqual(this IServiceSettingsModel source, IServiceSettingsModel target)
    {
        return target.Guid == source.Guid
        || (!string.IsNullOrEmpty(source.Name) && !string.IsNullOrEmpty(target.Name) && target.Name == source.Name)
        || (!string.IsNullOrEmpty(source.ServiceTypeName) && !string.IsNullOrEmpty(target.ServiceTypeName) &&
                    target.ServiceTypeName == source.ServiceTypeName);
    }
    public static bool FieldsEquals(this IServiceSettingsModel source, IImportServiceSettings other)
    {
        return other != null && source.Name == other.Name
           && ((string.IsNullOrEmpty(source.ServiceTypeName) && string.IsNullOrEmpty(other.ServiceTypeName))
             || string.Equals(source.ServiceTypeName, other.ServiceTypeName, StringComparison.CurrentCultureIgnoreCase))
            && ((string.IsNullOrEmpty(source.AssemblyPath) && string.IsNullOrEmpty(other.AssemblyPath))
             || string.Equals(source.AssemblyPath, other.AssemblyPath, StringComparison.CurrentCultureIgnoreCase))
            && ((string.IsNullOrEmpty(source.ServiceProviderPath) && string.IsNullOrEmpty(other.ServiceProviderPath))
             || string.Equals(source.ServiceProviderPath, other.ServiceProviderPath, StringComparison.CurrentCultureIgnoreCase))
             && ((string.IsNullOrEmpty(source.ImplementationTypeName) && string.IsNullOrEmpty(other.ImplementationTypeName))
             || string.Equals(source.ImplementationTypeName, other.ImplementationTypeName, StringComparison.CurrentCultureIgnoreCase));
    }
}
