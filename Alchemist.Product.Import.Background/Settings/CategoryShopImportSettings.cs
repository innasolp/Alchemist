using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background.Settings;

public class CategoryShopImportSettings : ShopImportSettings, ICategoryShopImportSettings
{
    public string CategorySourceUrl { get; set; }

    public override ShopSettingType ShopSettingType => ShopSettingType.Category;
}
