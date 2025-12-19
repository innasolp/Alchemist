using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.GoldApple.Model;
public class ProductCategoryData 
{
    [JsonPropertyName("url")]
    public string BaseUrl { get; set; }

    [JsonPropertyName("productCount")]
    public int Count { get; set; }

    [JsonPropertyName("cards")]
    public ProductCardItem[] Products { get; set; }
}
