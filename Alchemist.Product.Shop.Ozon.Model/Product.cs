using Alchemist.Import.Products.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public class Product : IJsonOnDeserialized, IProductItem
{
    [JsonPropertyName("widgetStates")]
    public JsonObject? WidgetStates { get; set; }

    [JsonIgnore]
    public string Name { get; set; }
    
    [JsonIgnore]
    public WebDescription? WebDescription { get; set; }

    [JsonIgnore]
    public WebCharacteristics? WebCharacteristics { get; set; }

    [JsonIgnore]
    public WebRichDescription? WebRichDescription { get; set; }

    string IProductItem.ItemId => WebCharacteristics.Sku;   

    string IProductItem.Shop => "ozon.ru";

    string[] IProductItem.Components => WebDescription.Components ?? WebCharacteristics.Components;

    string IProductItem.Brand => WebCharacteristics.Brand;

    string IProductItem.Country => WebCharacteristics.Country;

    string IProductItem.Comment => WebRichDescription.RichDescription;

    string IProductItem.ProductType => WebCharacteristics.Type;

    string[] IProductItem.Purposes => WebCharacteristics.PurposeTypes;

    string IProductItem.Articul => WebCharacteristics.Sku;

    string IProductItem.Currency { get; set; }
    double IProductItem.Price { get; set; }
    string IProductItem.ItemUrl { get; set; }
    string IProductItem.ApiUrl { get ; set; }

    public void OnDeserialized()
    {
        var webCharacteristicsValue = WidgetStates?.FirstOrDefault(p => p.Key.Contains("webCharacteristics"));
        if (webCharacteristicsValue?.Value != null)
        {
            WebCharacteristics = JsonSerializer.Deserialize<WebCharacteristics>(webCharacteristicsValue.Value.Value.ToString());

            if (!string.IsNullOrWhiteSpace(WebCharacteristics?.ProductTitle))
            {
                var splitByCharacteristic = WebCharacteristics.ProductTitle.Split(":");
                var productString = splitByCharacteristic.Length > 1 ? splitByCharacteristic[1] : splitByCharacteristic[0];
                Name = productString.Split(",")[0].Trim();
            }
        }

        var webDescriptionValue = WidgetStates?.FirstOrDefault(p => p.Key.Contains("webDescription") && p.Value != null
                                    && !p.Value.ToString().Contains("richAnnotation"));
        if (webDescriptionValue?.Value != null)
        {
            WebDescription = JsonSerializer.Deserialize<WebDescription>(webDescriptionValue.Value.Value.ToString());
        }

        var webRichDescriptionValue = WidgetStates?.FirstOrDefault(p => p.Key.Contains("webDescription") && p.Value != null
                                    && p.Value.ToString().Contains("richAnnotation"));
        if (webRichDescriptionValue?.Value != null)
        {
            WebRichDescription = JsonSerializer.Deserialize<WebRichDescription>(webRichDescriptionValue.Value.Value.ToString());
        }
    }
}


