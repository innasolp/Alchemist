using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.WebApp.Models;

internal class SettingsDataAdapterContainer(ISettingsDataAdapter productSettingsDataAdapter,
    ISettingsDataAdapter categorySettingsDataAdapter)
{
    private readonly ISettingsDataAdapter _productSettingsDataAdapter = productSettingsDataAdapter;
    private readonly ISettingsDataAdapter _categorySettingsDataAdapter = categorySettingsDataAdapter;

    public async Task<IShopImportSettingsModel?> GetShopImportSettingsAsync(int shopId, ShopSettingType shopSettingType)
    {
        switch (shopSettingType)
        {
            case ShopSettingType.Product:
                return await _productSettingsDataAdapter.GetShopImportSettings(shopId) as IShopImportSettingsModel;

            case ShopSettingType.Category:
                return await _categorySettingsDataAdapter.GetShopImportSettings(shopId) as IShopImportSettingsModel;

            default:
                return null;
        }
    }

    public async Task SaveAsync(IShopImportSettingsModel shopImportSettings)
    {
        switch(shopImportSettings.Type)
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
