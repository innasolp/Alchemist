using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.WebApp.Models;

internal class SettingsDataAdapterContainer(ISettingsDataAdapter productSettingsDataAdapter,
    ISettingsDataAdapter categorySettingsDataAdapter)
{
    private readonly ISettingsDataAdapter _productSettingsDataAdapter = productSettingsDataAdapter;
    private readonly ISettingsDataAdapter _categorySettingsDataAdapter = categorySettingsDataAdapter;

    public async Task<IShopImportSettings?> GetShopImportSettingsAsync(int shopId, ShopSettingType shopSettingType)
    {
        switch (shopSettingType)
        {
            case ShopSettingType.Product:
                return await _productSettingsDataAdapter.GetShopImportSettings(shopId);

            case ShopSettingType.Category:
                return await _categorySettingsDataAdapter.GetShopImportSettings(shopId);

            default:
                return null;
        }
    }

    public async Task SaveAsync(IShopImportSettings shopImportSettings)
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
