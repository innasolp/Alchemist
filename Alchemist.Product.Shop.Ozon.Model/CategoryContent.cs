using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public class CategoryContent
{
    [JsonPropertyName("tileLayout")]
    public string TileLayout { get; set; }

    [JsonPropertyName("items")]
    public ProductItem[] Items { get; set; }
}
