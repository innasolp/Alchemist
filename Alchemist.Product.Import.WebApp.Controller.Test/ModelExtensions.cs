using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Json.FileExtensions;
using System.Reflection;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

internal static class ModelExtensions
{
    internal static void FillServiceSettingsFields(this IServiceSettingsModel serviceSettings)
    {
        serviceSettings.ServiceTypeName = $"ServiceType_{serviceSettings.Name ?? ""}_{serviceSettings.ShopSettingsGuid}";
        serviceSettings.ServiceProviderPath = $"ServiceProviderPath_{serviceSettings.Name ?? ""}_{serviceSettings.ShopSettingsGuid}";
        serviceSettings.AssemblyPath = $"AsseblyPath_{serviceSettings.Name ?? ""}_{serviceSettings.ShopSettingsGuid}";
        serviceSettings.ImplementationTypeName = $"ImplementationType_{serviceSettings.Name ?? ""}_{serviceSettings.ShopSettingsGuid}";
    }

    internal static IShopServicesSettingsModel GetCopy(this IShopServicesSettingsModel shopSettings)
    {
        ShopSettingsModel newShopSettings = shopSettings.ShopSettingType == Alchemist.Import.Settings.Interfaces.ShopSettingType.Product
            ? new ProductShopSettingsModel(shopSettings.ShopId, shopSettings.Id, shopSettings.ShopGuid)
            : new CategoryShopSettingsModel(shopSettings.ShopId, shopSettings.Id, shopSettings.ShopGuid);
        newShopSettings.Update(shopSettings);
        return newShopSettings;
    }
    
    internal static IServiceSettingsModel GetCopy(this IModelFactory modelFactory, IServiceSettingsModel serviceSettings)
    {
        var newServiceSettings = modelFactory.CreateServiceSettingsModel(serviceSettings.ShopId,
            serviceSettings.Id,
            serviceSettings.ParentSettingsId.Value,
            serviceSettings.ShopGuid,
            serviceSettings.ShopSettingsGuid,  
            serviceSettings.Name);
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
