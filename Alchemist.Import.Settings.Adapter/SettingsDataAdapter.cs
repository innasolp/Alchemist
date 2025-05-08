using Alchemist.DataService.Interfaces;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;

namespace Alchemist.Import.Settings.Adapter;

public class SettingsDataAdapter<TProductShopImportSettings, TCategoryShopImportSettings, TImportServiceSettings>(IShopSettingsDataService shopSettingsDataService)
    : ISettingsDataAdapter
    where TProductShopImportSettings : class, IProductShopImportSettings
    where TCategoryShopImportSettings : class, ICategoryShopImportSettings
    where TImportServiceSettings : class, IImportServiceSettings, new()
{
    private readonly IShopSettingsDataService _shopSettingsDataService = shopSettingsDataService;    

    public async Task<IShopImportSettings?> GetShopImportSettings(int shopId, ShopSettingType shopSettingType)
    {
        var shopSettings = await _shopSettingsDataService.GetShopSettings(shopId, shopSettingType);
        if (shopSettings == null) return null;

        return await GetShopImportSettings(shopSettings);
    }

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName)
    {
        var shopSettings = await _shopSettingsDataService.GetShopSettings(shopSettingsName);
        if (shopSettings == null) return null;
        return await GetShopImportSettings(shopSettings);
    }

    private async Task<IShopImportSettings?> GetShopImportSettings(IShopSettings shopSettings)
    {
        IShopImportSettings shopSettingsModel = shopSettings.Type == ShopSettingType.Product
           ? shopSettings.ToShopImportSettings<TProductShopImportSettings>()
           : shopSettings.ToShopImportSettings<TCategoryShopImportSettings>();

        var services = await _shopSettingsDataService.GetChildSettings(shopSettings.Id);
        var serviceModels = services.Select(s => s.ToImportServiceSettings<TImportServiceSettings>()).ToList();

        foreach (var serviceModel in serviceModels)
            shopSettingsModel.UpdateServiceSettings<TImportServiceSettings>(serviceModel);

        return shopSettingsModel;
    }

    public async Task Save(IShopImportSettings shopSettingsModel)
    {
        var shopSettings = shopSettingsModel.ToEntity();

        var services = (from serviceModel in shopSettingsModel.Services.OfType<IImportServiceSettings>()
                        let service = serviceModel.ToEntity()
                        select service).ToList();
        await _shopSettingsDataService.SaveShopSettings(shopSettings, services);
    }
}
