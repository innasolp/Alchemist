using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model;

public class ProductShopSettingsModel : ShopSettingsModel, IProductShopImportSettings
{
    public string? ProductUrl { get; set; }
    public string? CategoryUrl { get; set; }
    public int? PageProductCount { get; set; }

    public override ShopSettingType ShopSettingType => ShopSettingType.Product;
}
