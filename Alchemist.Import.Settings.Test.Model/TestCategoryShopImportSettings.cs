using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Test.Model;

public class TestCategoryShopImportSettings : TestShopImportSettings, ICategoryShopImportSettings
{
    public string CategorySourceUrl { get; set; }

    public override ShopSettingType Type => ShopSettingType.Category;
}
