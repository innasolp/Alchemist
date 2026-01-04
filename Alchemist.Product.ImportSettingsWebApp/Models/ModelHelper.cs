using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.Interfaces;
namespace Alchemist.Product.ImportSettingsWebApp.Models;

internal static class ModelHelper
{
    public static ShopImportSettingsModel CreateShopImportSettingsModel(int shopId, ShopSettingType shopSettingType)
    {
        return shopSettingType switch
        {
            ShopSettingType.Product => new ProductShopImportSettingsModel() { ShopId = shopId },
            ShopSettingType.Category => new CategoryShopImportSettingsModel() { ShopId = shopId },
            _ => throw new InvalidOperationException($"Invalid settings type {shopSettingType}"),
        };
    }

    internal static async Task<ShopImportSettingsModel> GetShopImportSettingsModel(this SettingsDataAdapterContainer settingsDataAdapter,
        int? shopId, ShopSettingType shopSettingsType, CancellationToken cancellationToken = default)
    {
        return shopId != null
            ? await settingsDataAdapter.GetShopImportSettingsAsync((int)shopId, shopSettingsType, cancellationToken)
                ?? CreateShopImportSettingsModel(shopId ?? 0, shopSettingsType)
            : CreateShopImportSettingsModel(shopId ?? 0, shopSettingsType);
    }

    internal static async Task<ShopImportSettingsModel> GetShopImportSettingsModel(this SettingsFacade settingsDataAdapter,
        int? shopId, ShopSettingType shopSettingsType, CancellationToken cancellationToken = default)
    {
        return shopId != null
            ? await settingsDataAdapter.GetShopImportSettingsAsync((int)shopId, shopSettingsType, cancellationToken)
                ?? CreateShopImportSettingsModel(shopId ?? 0, shopSettingsType)
            : CreateShopImportSettingsModel(shopId ?? 0, shopSettingsType);
    }
}