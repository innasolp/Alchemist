using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

internal class SettingsDataAdapterContainer(ISettingsDataAdapter productSettingsDataAdapter,
    ISettingsDataAdapter categorySettingsDataAdapter)
{
    private readonly ISettingsDataAdapter _productSettingsDataAdapter = productSettingsDataAdapter;
    private readonly ISettingsDataAdapter _categorySettingsDataAdapter = categorySettingsDataAdapter;

    public async Task<ShopImportSettingsModel?> GetShopImportSettingsAsync(int shopId, ShopSettingType shopSettingType)
    {
        switch (shopSettingType)
        {
            case ShopSettingType.Product:
                return await _productSettingsDataAdapter.GetShopImportSettings(shopId) as ShopImportSettingsModel;

            case ShopSettingType.Category:
                return await _categorySettingsDataAdapter.GetShopImportSettings(shopId) as ShopImportSettingsModel;

            default:
                return null;
        }
    }

    public async Task SaveAsync(ShopImportSettingsModel shopImportSettings)
    {
        switch(shopImportSettings.ShopSettingType)
        {
            case ShopSettingType.Product:
               await _productSettingsDataAdapter.Save(shopImportSettings);
                break;

            case ShopSettingType.Category:
                await _categorySettingsDataAdapter.Save(shopImportSettings);
                break;
        }
    }
}
