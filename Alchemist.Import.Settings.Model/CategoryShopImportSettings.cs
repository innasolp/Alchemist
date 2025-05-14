using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Model;

public class CategoryShopImportSettings : ShopImportSettings, ICategoryShopImportSettings
{
    protected override ShopSettingType ShopSettingType => ShopSettingType.Category;
}
