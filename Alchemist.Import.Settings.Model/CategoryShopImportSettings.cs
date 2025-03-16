using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Model;

public class CategoryShopImportSettings : ShopImportSettings, ICategoryShopImportSettings
{
    protected override ShopSettingType ShopSettingType => ShopSettingType.Category;
}
