using Alchemist.Product.Interfaces;
namespace Alchemist.Product.ImportSettingsWebApp.Models;

internal static class ModelHelper
{
    public static ShopImportSettingsModel CreateShopImportSettingsModel(ShopSettingType shopSettingType)
    {
        return shopSettingType switch
        {
            ShopSettingType.Product => new ProductShopImportSettingsModel(),
            ShopSettingType.Category => new CategoryShopImportSettingsModel(),
            _ => throw new InvalidOperationException($"Invalid settings type {shopSettingType}"),
        };
    }

    internal static async Task<ShopImportSettingsModel> GetShopImportSettingsModel(this SettingsDataAdapterContainer settingsDataAdapter,
        int? shopId, ShopSettingType shopSettingsType)
    {
        return shopId != null
            ? await settingsDataAdapter.GetShopImportSettingsAsync((int)shopId, shopSettingsType)
                ?? ModelHelper.CreateShopImportSettingsModel(shopSettingsType)
            : ModelHelper.CreateShopImportSettingsModel(shopSettingsType);
    }
}
