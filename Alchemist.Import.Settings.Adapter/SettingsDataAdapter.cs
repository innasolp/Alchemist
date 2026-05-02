using Alchemist.Import.Settings.Extensions;
using System.Text.Json;
using Import.Settings.Interfaces;
using ShopSettings.Interfaces;

namespace Alchemist.Import.Settings.DataAdapter;

public class SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(IShopSettingsDataService shopSettingsDataService, ShopSettingType shopSettingType)
    : ISettingsDataAdapter
    where TShopImportSettings : class, IShopImportSettings, IShopSettings, new()
    where TImportServiceSettings : class, IServiceSettings, IShopSettings, new()
{
    private readonly IShopSettingsDataService _shopSettingsDataService = shopSettingsDataService;

    private readonly ShopSettingType _shopSettingType = shopSettingType;

    public async Task<IShopImportSettings?> GetShopImportSettings(int shopId, CancellationToken cancellationToken = default)
    {
        var shopSettings = await _shopSettingsDataService.GetShopSettings(shopId, _shopSettingType, cancellationToken);
        if (shopSettings == null) return null;
        var services = await _shopSettingsDataService.GetChildSettings(shopSettings.Id, cancellationToken);
        return GetShopImportSettings(shopSettings, services);
    }

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName, CancellationToken cancellationToken = default)
    {
        var shopSettings = await _shopSettingsDataService.GetShopSettings(shopSettingsName, cancellationToken);
        if (shopSettings == null) return null;
        var services = await _shopSettingsDataService.GetChildSettings(shopSettings.Id, cancellationToken);
        return GetShopImportSettings(shopSettings, services);
    }

    private static TShopImportSettings GetShopImportSettings(IShopSettings shopSettings, IEnumerable<IShopSettings> services)
    {
        var shopSettingsModel = shopSettings.ToShopImportSettings<TShopImportSettings>();
        SetShopSettings(shopSettingsModel, shopSettings);
        //todo ShouldSerialize =>(..,..) => false not working
        shopSettingsModel.Services.Clear();

        var serviceModels = services.ToDictionary(s => s.Name, s =>
        {
            var service = s.ToImportServiceSettings<TImportServiceSettings>();
            SetShopSettings(service, s);
            return service;
        })
            .ToList();

        foreach (var serviceModel in serviceModels)
            shopSettingsModel.UpdateServices(serviceModel.Key, serviceModel.Value);

        return shopSettingsModel;
    }

    private static void SetShopSettings(IShopSettings target, IShopSettings source)
    {
        target.Id = source.Id;
        target.ParentSettingsId = source.ParentSettingsId;
        target.Type = source.Type;
        target.ShopId = source.ShopId;
        target.Name = source.Name;
    }

    public async Task<TShopImportSettings> Save(TShopImportSettings shopSettingsModel, CancellationToken cancellationToken = default)
    {
        var shopSettings = shopSettingsModel.To<TShopImportSettings>() as IShopSettings;
        shopSettings.JsonValue = JsonSerializer.Serialize(shopSettingsModel,
            EntityExtensions.GetDefaultImportSettingsSerializationOptions<TShopImportSettings>());

        var services = new List<IShopSettings>();
        foreach (var (serviceModel, service) in from serviceModel in shopSettingsModel.Services.OfType<KeyValuePair<string, TImportServiceSettings>>()
                                                let service = serviceModel.Value.ToShopSettings()
                                                select (serviceModel, service))
        {
            service.JsonValue = JsonSerializer.Serialize(serviceModel.Value);
            if(string.IsNullOrEmpty(service.Name)) service.Name = serviceModel.Key;
            services.Add(service);
        }

        var result = await _shopSettingsDataService.SaveShopSettings(shopSettings, services,cancellationToken);
        
        var savedShopImportSettings = GetShopImportSettings(result.ShopSettings, result.Services);
        
        return savedShopImportSettings;
    }

    public async Task<Dictionary<string,IShopImportSettings>> GetAllShopImportSettings(CancellationToken cancellationToken = default)
    {
        var allParents = await _shopSettingsDataService.GetAllParentShopSettings(cancellationToken);
        var allShopImportSettings = new Dictionary<string,IShopImportSettings>();
        foreach (var shopSettings in allParents)
        {
            var services = await _shopSettingsDataService.GetChildSettings(shopSettings.Id, cancellationToken);
            var shopImportSettings = GetShopImportSettings(shopSettings, services);
            
            if(shopImportSettings != null)
                allShopImportSettings.Add(shopSettings.Name, shopImportSettings);
        }
        return allShopImportSettings;
    }

    async Task<IShopImportSettings> ISettingsDataAdapter.Save(IShopImportSettings shopSettingsModel, CancellationToken cancellationToken)
    {
        if (shopSettingsModel is not TShopImportSettings shopImportSettings)
            throw new InvalidOperationException($"Shop settings type {shopSettingsModel.GetType().Name} not implement {typeof(TShopImportSettings).Name}.");

        return await Save(shopImportSettings, cancellationToken);
    }
}