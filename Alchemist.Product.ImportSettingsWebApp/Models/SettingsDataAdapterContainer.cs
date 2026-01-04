using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

internal class SettingsDataAdapterContainer(ISettingsDataAdapter productSettingsDataAdapter,
    ISettingsDataAdapter categorySettingsDataAdapter)
{
    private readonly ISettingsDataAdapter _productSettingsDataAdapter = productSettingsDataAdapter;
    private readonly ISettingsDataAdapter _categorySettingsDataAdapter = categorySettingsDataAdapter;

    public async Task<ShopImportSettingsModel?> GetShopImportSettingsAsync(int shopId, ShopSettingType shopSettingType, CancellationToken cancellationToken = default)
    {
        switch (shopSettingType)
        {
            case ShopSettingType.Product:
                return await _productSettingsDataAdapter.GetShopImportSettings(shopId, cancellationToken : cancellationToken) as ShopImportSettingsModel;

            case ShopSettingType.Category:
                return await _categorySettingsDataAdapter.GetShopImportSettings(shopId, cancellationToken) as ShopImportSettingsModel;

            default:
                throw new InvalidOperationException($"Invalid shopSettingType {shopSettingType}");
        }
    }

    public async Task SaveAsync(ShopImportSettingsModel shopImportSettings, CancellationToken cancellationToken = default)
    {
        switch(shopImportSettings.ShopSettingType)
        {
            case ShopSettingType.Product:
               await _productSettingsDataAdapter.Save(shopImportSettings, cancellationToken);
                break;

            case ShopSettingType.Category:
                await _categorySettingsDataAdapter.Save(shopImportSettings, cancellationToken);
                break;
        }
    }
}
