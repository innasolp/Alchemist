using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Model;

public class ProductShopImportSettings : ShopImportSettings, IProductShopImportSettings
{   
    public string? ProductUrl { get; set; }

    public string? CategoryUrl { get; set; }

    public int? PageProductCount { get; set; }
    
}
