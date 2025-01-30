namespace Alchemist.Product.Import.Background;

public class ShopProductsSettings : ShopSettings
{
    public string? ProductUrl { get; set; }
    public string? CategoryUrl { get; set; }

    public int? PageProductCount { get; set; }
}
