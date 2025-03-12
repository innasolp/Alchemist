using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Model;

public class ProductShopImportSettings : ShopImportSettings, IProductShopImportSettings
{   
    public string? ProductUrl { get; set; }

    public string? CategoryUrl { get; set; }

    public int? PageProductCount { get; set; }

    protected override ShopSettingType ShopSettingType => ShopSettingType.Product;
}
