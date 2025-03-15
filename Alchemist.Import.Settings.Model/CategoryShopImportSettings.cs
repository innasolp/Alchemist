using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Model;

public class CategoryShopImportSettings : ShopImportSettings, ICategoryShopImportSettings
{
    public string ShopName { get; set; }

    protected override ShopSettingType ShopSettingType => ShopSettingType.Category;
}
