using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Infrastructure;

internal class ServiceSettingsFacade(ISettingsDataAdapter productSettingsDataAdapter,
    ISettingsDataAdapter categorySettingsDataAdapter, Controller controller)
    : SettingsFacade(productSettingsDataAdapter, categorySettingsDataAdapter, controller)
{
    private static ServiceSettingsModel GetServiceSettings(ShopImportSettingsModel shopImportSettings, string? serviceName = null, Guid? guid = null)
    {
        var service = serviceName?.IsPrimaryServiceName() == true
            ? shopImportSettings.GetService<ServiceSettingsModel>(serviceName) ?? new ServiceSettingsModel { Name = serviceName }
            : !string.IsNullOrEmpty(serviceName) && shopImportSettings.Services.TryGetValue(serviceName, out var serviceSettings) && serviceSettings != null
                ? serviceSettings
                : guid != null
                     ? shopImportSettings.Services.FirstOrDefault(s => s.Value.Guid == guid).Value ?? new ServiceSettingsModel()
                     : new ServiceSettingsModel();

        if (!string.IsNullOrEmpty(serviceName)) service.Name = serviceName;
        service.ParentSettingsId = shopImportSettings.Id;
        service.ShopId = shopImportSettings.ShopId;

        return service;
    }
    public async Task<ServiceSettingsModel> GetServiceSettingsAsync(string serviceName, int shopId, ShopSettingType shopSettingsType)
    {
        var shopImportSettings = await GetCurrentShopImportSettingsAsync(shopId, shopSettingsType);
        return GetServiceSettings(shopImportSettings, serviceName, null);
    }

    public async Task<ServiceSettingsModel> GetServiceSettingsAsync(Guid guid, int shopId, ShopSettingType shopSettingsType)
    {
        var shopImportSettings = await GetCurrentShopImportSettingsAsync(shopId, shopSettingsType);
        return GetServiceSettings(shopImportSettings, null, guid);
    }

    private async Task<ServiceSettingsModel> SaveServiceSettingsAsync(int shopId, ShopSettingType shopSettingsType, ServiceSettingsModel data,
        Func<ShopImportSettingsModel, ServiceSettingsModel> getCurrentService)
    {
        var shopImportSettings = await GetCurrentShopImportSettingsAsync(shopId, shopSettingsType);

        if (shopImportSettings == null //todo
            || shopImportSettings.ShopImportSettingsIsEmpty())
        {
            shopImportSettings?.Services.Add(data.Name, data);
            Controller.HttpContext.Session.SetImportSettingToSession(shopImportSettings);
            return data;
        }

        var existingService = getCurrentService(shopImportSettings);
        //shopImportSettings.GetService<ServiceSettingsModel>(data.Name) ?? shopImportSettings.GetService(data.Guid);

        ServiceSettingsModel result;
        if (existingService == null)
        {
            shopImportSettings.Services.Add(data.Name, data);
            result = data;
        }
        else
        {
            existingService.Updateservice(data);
            result = existingService;
        }

        Controller.HttpContext.Session.SetImportSettingToSession(shopImportSettings);

        return result;
    }

    public async Task<ServiceSettingsModel> SaveServiceSettingsAsync(int shopId, ShopSettingType shopSettingsType, string name, ServiceSettingsModel data)
    {
        return await SaveServiceSettingsAsync(shopId, shopSettingsType, data,
            (shopImportSettings) => shopImportSettings.GetService<ServiceSettingsModel>(name));
    }

    public async Task<ServiceSettingsModel> SaveServiceSettingsAsync(int shopId, ShopSettingType shopSettingsType, Guid guid, ServiceSettingsModel data)
    {
        return await SaveServiceSettingsAsync(shopId, shopSettingsType, data,
            (shopImportSettings) => shopImportSettings.GetService(guid));
    }

    public async Task<bool> IsChanged(int shopId, ShopSettingType shopSettingsType, ServiceSettingsModel data)
    {
        var shopImportSettings = await GetCurrentShopImportSettingsAsync(shopId, shopSettingsType);
        if (shopImportSettings == null)
            return !data.IsEmpty();

        var existingService = shopImportSettings.GetService<ServiceSettingsModel>(data.Name) ??
            shopImportSettings.GetService(data.Guid);

        return existingService != null && !data.ServiceEquals(existingService);
    }
}
