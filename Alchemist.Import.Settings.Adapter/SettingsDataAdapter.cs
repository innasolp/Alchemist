using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;

namespace Alchemist.Import.Settings.DataAdapter;

public class SettingsDataAdapter<TProductShopImportSettings, TCategoryShopImportSettings, TImportServiceSettings>(IShopSettingsDataService shopSettingsDataService)
    : ISettingsDataAdapter
    where TProductShopImportSettings : class, IProductShopImportSettings
    where TCategoryShopImportSettings : class, ICategoryShopImportSettings
    where TImportServiceSettings : class, IImportServiceSettings
{
    private readonly IShopSettingsDataService _shopSettingsDataService = shopSettingsDataService;    

    public async Task<IShopImportSettings?> GetShopImportSettings(int shopId, ShopSettingType shopSettingType)
    {
        var shopSettings = await _shopSettingsDataService.GetShopSettings(shopId, (Product.Interfaces.ShopSettingType) (int)shopSettingType);
        if (shopSettings == null) return null;

        return await GetShopImportSettings(shopSettings);
    }

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName)
    {
        var shopSettings = await _shopSettingsDataService.GetShopSettings(shopSettingsName);
        if (shopSettings == null) return null;
        return await GetShopImportSettings(shopSettings);
    }

    private async Task<IShopImportSettings?> GetShopImportSettings(Product.Interfaces.IShopSettings shopSettings)
    {
        IShopImportSettings shopSettingsModel = shopSettings.Type == Product.Interfaces.ShopSettingType.Product
           ? shopSettings.ToShopImportSettings<TProductShopImportSettings>()
           : shopSettings.ToShopImportSettings<TCategoryShopImportSettings>();

        var services = await _shopSettingsDataService.GetChildSettings(shopSettings.Id);
        var serviceModels = services.Select(s => s.ToImportServiceSettings<TImportServiceSettings>()).ToList();

        foreach (var serviceModel in serviceModels)
            shopSettingsModel.UpdateServiceSettings(serviceModel.Name, serviceModel);

        return shopSettingsModel;
    }

    public async Task Save(IShopImportSettings shopSettingsModel)
    {
        var shopSettings = shopSettingsModel.ToEntity();

        var services = (from serviceModel in shopSettingsModel.Services.OfType<IImportServiceSettings>()
                        let service = serviceModel.ToEntity()
                        select service).ToList();
        //todo add primary services if need
        await _shopSettingsDataService.SaveShopSettings(shopSettings, services);
    }

    public async Task<List<IShopImportSettings>> GetAllShopImportSettings()
    {
        var allParents = await _shopSettingsDataService.GetAllParentShopSettings();
        var shopImportSettings = new List<IShopImportSettings>();
        foreach (var shopSettings in allParents)
        {
            shopImportSettings.Add(await GetShopImportSettings(shopSettings));
        }
        return shopImportSettings;
    }

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName, ShopSettingType shopSettingType)
    {
        return await GetShopImportSettings(shopSettingsName);
    }
}
