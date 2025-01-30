using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public class Layout
{
    [JsonPropertyName("widgetToken")]
    public string WidgetToken { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
}
