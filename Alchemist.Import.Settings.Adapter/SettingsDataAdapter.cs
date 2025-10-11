using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Interfaces;
using Alchemist.Product.Entities;
using System.Text.Json;

namespace Alchemist.Import.Settings.DataAdapter;

public class SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(IShopSettingsDataService shopSettingsDataService, ShopSettingType shopSettingType)
    : ISettingsDataAdapter
    where TShopImportSettings : class, IShopImportSettings, IShopSettings
    where TImportServiceSettings : class, IServiceSettings, IShopSettings
{
    private readonly IShopSettingsDataService _shopSettingsDataService = shopSettingsDataService;

    private readonly ShopSettingType _shopSettingType = shopSettingType;

    public async Task<IShopImportSettings?> GetShopImportSettings(int shopId)
    {
        var shopSettings = await _shopSettingsDataService.GetShopSettings(shopId, _shopSettingType);
        if (shopSettings == null) return null;

        return await GetShopImportSettings(shopSettings);
    }

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName)
    {
        var shopSettings = await _shopSettingsDataService.GetShopSettings(shopSettingsName);
        if (shopSettings == null) return null;
        return await GetShopImportSettings(shopSettings);
    }

    private async Task<TShopImportSettings?> GetShopImportSettings(IShopSettings shopSettings)
    {
        var shopSettingsModel = shopSettings.ToShopImportSettings<TShopImportSettings>();
        SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>.SetShopSettings(shopSettingsModel, shopSettings);

        var services = await _shopSettingsDataService.GetChildSettings(shopSettings.Id);
        var serviceModels = services.ToDictionary(s=>s.Name, s =>
        {
            var service = s.ToImportServiceSettings<TImportServiceSettings>();
            SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>.SetShopSettings(service, s);
            return service;
        })
            .ToList();

        foreach (var serviceModel in serviceModels)
            shopSettingsModel.UpdateServiceSettings(serviceModel.Key, serviceModel.Value);

        return shopSettingsModel;
    }

    private static void SetShopSettings(IShopSettings target, IShopSettings source)
    {
        target.Id = source.Id;
        target.ParentSettingsId = source.ParentSettingsId;
        target.Type = source.Type;
        target.ShopId = source.ShopId;
    }

    public async Task Save(TShopImportSettings shopSettingsModel)
    {
        var shopSettings = shopSettingsModel.To<ShopSettings>() as IShopSettings;
        shopSettings.JsonValue = JsonSerializer.Serialize(shopSettingsModel);

        var services = new List<IShopSettings>();
        foreach (var (serviceModel, service) in from serviceModel in shopSettingsModel.Services.OfType<TImportServiceSettings>()
                                                let service = serviceModel.To<ShopSettings>() as IShopSettings
                                                select (serviceModel, service))
        {
            service.JsonValue = JsonSerializer.Serialize(serviceModel);
            services.Add(service);
        }

        //todo add primary services if need
        await _shopSettingsDataService.SaveShopSettings(shopSettings, services);
    }

    public async Task<Dictionary<string,IShopImportSettings>> GetAllShopImportSettings()
    {
        var allParents = await _shopSettingsDataService.GetAllParentShopSettings();
        var allShopImportSettings = new Dictionary<string,IShopImportSettings>();
        foreach (var shopSettings in allParents)
        {
            var shopImportSettings = await GetShopImportSettings(shopSettings);
            
            if(shopImportSettings != null)
                allShopImportSettings.Add(shopSettings.Name, shopImportSettings);
        }
        return allShopImportSettings;
    }

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName, ShopSettingType shopSettingType)
    {
        return await GetShopImportSettings(shopSettingsName);
    }

    Task ISettingsDataAdapter.Save(IShopImportSettings shopSettingsModel)
    {
        if (shopSettingsModel is not TShopImportSettings shopImportSettings)
            throw new InvalidOperationException($"Shop settings type {shopSettingsModel.GetType().Name} not implement {typeof(TShopImportSettings).Name}.");

        return Save(shopImportSettings);
    }
}
