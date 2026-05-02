using Alchemist.Import.Settings.Category;
using ShopSettings.Interfaces;

namespace Alchemist.Import.Settings.Test.Model;

public class TestCategoryShopImportSettings : TestShopImportSettings, ICategoryShopImportSettings
{
    public string CategorySourceUrl { get; set; }

    public override ShopSettingType Type => ShopSettingType.Category;
}
