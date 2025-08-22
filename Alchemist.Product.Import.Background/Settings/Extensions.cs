using Alchemist.Product.Interfaces;
using Alchemist.Import.Settings.Extensions;

namespace Alchemist.Product.Import.Background.Settings;

internal static class Extensions
{
    public static async Task<ShopImportSettings> GetShopImportSettingsAsync(this IShopSettings shopSettings, IEnumerable<IShopSettings> children)
    {
        return shopSettings.Type == ShopSettingType.Product
            ? await shopSettings.GetShopImportSettings<ProductShopImportSettings, ImportServiceSettings>(children) as ShopImportSettings
            : await shopSettings.GetShopImportSettings<CategoryShopImportSettings, ImportServiceSettings>(children);
    }
    
}
