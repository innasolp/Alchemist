using Alchemist.Product.Interfaces;
using Alchemist.Import.Settings.Extensions;

namespace Alchemist.Product.Import.Background.Settings;

internal static class Extensions
{
    public static ShopImportSettings GetShopImportSettings(this IShopSettings shopSettings, IEnumerable<IShopSettings> children)
    {
        return shopSettings.Type == ShopSettingType.Product
            ? shopSettings.GetShopImportSettings<ProductShopImportSettings, ImportServiceSettings>(children) 
            : shopSettings.GetShopImportSettings<CategoryShopImportSettings, ImportServiceSettings>(children);
    }
    
}
