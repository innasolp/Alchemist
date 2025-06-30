using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Background.Settings;

public class CategoryShopImportSettings : ShopImportSettings, ICategoryShopImportSettings
{
    public string CategorySourceUrl { get; set; }

    protected override ShopSettingType ShopSettingType => ShopSettingType.Category;
}
