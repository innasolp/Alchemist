using Alchemist.DataService.Interfaces;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;
using System.Collections;

namespace Alchemist.Import.Settings.Adapter;

internal class SettingsDataAdapter<TProductShopImportSettings, TCategoryShopImportSettings, TImportServiceSettings>(IShopSettingsDataService shopSettingsDataService)
    : ISettingsDataAdapter<TProductShopImportSettings, TCategoryShopImportSettings, TImportServiceSettings>
    where TProductShopImportSettings : class, IProductShopImportSettings
    where TCategoryShopImportSettings : class, ICategoryShopImportSettings
    where TImportServiceSettings : class, IImportServiceSettings, new()
{
    private readonly IShopSettingsDataService _shopSettingsDataService = shopSettingsDataService;

    public async Task<IShopImportSettings?> GetShopSettings(int shopId, ShopSettingType shopSettingType)
    {
        var shopSettings = await _shopSettingsDataService.GetShopSettings(shopId, shopSettingType);
        if (shopSettings == null) return null;

        IShopImportSettings shopSettingsModel = shopSettingType == ShopSettingType.Product
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
