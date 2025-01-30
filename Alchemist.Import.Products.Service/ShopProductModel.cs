using Alchemist.Import.Products.Interfaces;
using System.Text.Json.Serialization;

namespace Alchemist.Import.Products.Service;

public class ShopProductModel: IShopProductModel
{
    [JsonPropertyName("shop")]
    public string Shop { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("components")]
    public string[] Components { get; set; }

    [JsonPropertyName("band")]
    public string Brand { get; set; }

    [JsonPropertyName("Country")]
    public string Country { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; }

    [JsonPropertyName("productType")]
    public string ProductType { get; set; }

    [JsonPropertyName("itemId")]
    public string ItemId { get; set; }

    [JsonPropertyName("purposes")]
    public string[] Purposes { get; set; }

    [JsonPropertyName("articul")]
    public string Articul { get; set; }
    public int ShopId { get; set; }
    public int CategoryItemId { get; set; }
    public string Currency { get; set; }
    public double Price { get; set; }
    public string ItemUrl { get; set; }
    public string ApiUrl { get; set; }
}
