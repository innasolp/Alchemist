using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Json.FileExtensions;
using System.Reflection;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

internal static class ModelExtensions
{
    internal static void FillShopSettingsFields(this ShopSettingsModel shopSettings)
    {
        shopSettings.Name = $"{shopSettings.ShopSettingType}_{Guid.NewGuid}";
        shopSettings.Perfomance = true;
    }

    internal static void FillServiceSettingsFields(this ServiceSettingsModel serviceSettings)
    {
        serviceSettings.ServiceTypeName = $"ServiceType_{serviceSettings.Name ?? ""}_{serviceSettings.ShopSettingsGuid}";
        serviceSettings.ServiceProviderPath = $"ServiceProviderPath_{serviceSettings.Name ?? ""}_{serviceSettings.ShopSettingsGuid}";
        serviceSettings.AssemblyPath = $"AsseblyPath_{serviceSettings.Name ?? ""}_{serviceSettings.ShopSettingsGuid}";
        serviceSettings.ImplementationTypeName = $"ImplementationType_{serviceSettings.Name ?? ""}_{serviceSettings.ShopSettingsGuid}";
    }

    internal static ShopSettingsModel GetCopy(this ShopSettingsModel shopSettings)
    {
        var newShopSettings = ModelHelper.CreateShopSettings(shopSettings.ShopGuid, shopSettings.ShopId, shopSettings.ShopSettingType);
        newShopSettings.Update(shopSettings, true);
        return newShopSettings;
    }
    
    internal static ServiceSettingsModel GetCopy(this ServiceSettingsModel serviceSettings)
    {
        var newServiceSettings = ModelHelper.CreateServiceSettingsModel(serviceSettings.ShopGuid, serviceSettings.ShopId, serviceSettings.ShopSettingsGuid,  serviceSettings.Name);
        newServiceSettings.Update(serviceSettings);
        return newServiceSettings;
    }

    internal static async Task<T> ReadFromFileAsync<T>(this string fileName)
        where T : class
    {
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";
        return await filePath.ReadFromJsonFileAsync<T>() ??
            throw new InvalidOperationException($"Can't deserialize file {fileName}");
    }
}
