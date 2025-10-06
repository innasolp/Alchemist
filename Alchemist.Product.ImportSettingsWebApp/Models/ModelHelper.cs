using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

public static class ModelHelper
{
    public static ShopImportSettingsModel GetShopImportSettingsModel(ShopSettingType shopSettingType)
    {
        return shopSettingType switch
        {
            ShopSettingType.Product => new ProductShopImportSettingsModel(),
            ShopSettingType.Category => new CategoryShopImportSettingsModel(),
            _ => throw new InvalidOperationException($"Invalid settings type {shopSettingType}"),
        };
    }
}
