using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public class LayoutItem
{
    [JsonPropertyName("widgetToken")]
    public string WidgetToken { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("stateId")]
    public string StateId { get; set; }
}
