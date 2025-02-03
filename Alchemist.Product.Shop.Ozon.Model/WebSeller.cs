using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public class WebSeller
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
}
