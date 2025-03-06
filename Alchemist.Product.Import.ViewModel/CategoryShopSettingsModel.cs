using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model;

public class CategoryShopSettingsModel : ShopSettingsModel, IShopImportSettings
{
    public override ShopSettingType ShopSettingType => ShopSettingType.Category;
}
