using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Model;

public class ProductShopImportSettings : ShopImportSettings, IProductShopImportSettings
{
    public ProductShopImportSettings() { }

    public string? ProductUrl { get; set; }

    public string? CategoryUrl { get; set; }

    public int? PageProductCount { get; set; }

    public static ProductShopImportSettings CreateProductShopImportSettings(IShopSettings shopSettings, Func<int, IShopSettings> getServiceSettingsById)
    {
        var productShopImportSettings = Create<ProductShopImportSettings>(shopSettings, getServiceSettingsById);
        return productShopImportSettings;
    }
}
