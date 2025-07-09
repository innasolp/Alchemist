using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Test.Model;

public class TestCategoryShopImportSettings : TestShopImportSettings, ICategoryShopImportSettings
{
    public string CategorySourceUrl { get; set; }

    protected override ShopSettingType ShopSettingType => ShopSettingType.Category;
}
