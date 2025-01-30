using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.GoldApple.Model;
public class ProductCategoryData 
{
    [JsonPropertyName("url")]
    public string BaseUrl { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("products")]
    public ProductInCategory[] Products { get; set; }
}
