using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Model;

public class CategoryShopImportSettings : ShopImportSettings, ICategoryShopImportSettings
{
    public string CategorySourceUrl { get; set; }

    protected override ShopSettingType ShopSettingType => ShopSettingType.Category;
}
