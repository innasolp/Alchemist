using Alchemist.Import.Products.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public class Category : IJsonOnDeserialized, ICategoryProducts
{
    [JsonPropertyName("widgetStates")]
    public JsonObject? WidgetStates { get; set; }

    [JsonIgnore]
    public Shared? Shared { get; set; }

    [JsonPropertyName("shared")]
    public string? SharedContent { get; set; }

    [JsonIgnore]
    public CategoryContent? CategoryContent { get; set; }

    [JsonIgnore]
    ICategoryProductItem[] ICategoryProducts.CategoryProductItems { get => CategoryContent?.Items.OfType<ICategoryProductItem>().ToArray() ?? []; }

    [JsonIgnore]
    int ICategoryProducts.TotalCount => Shared?.Catalog?.TotalFound ?? 0;

    [JsonPropertyName("layout")]
    public LayoutItem[] LayoutItems { get; set; } = [];

    public void OnDeserialized()
    {
        var searchResultsV2 = WidgetStates?.FirstOrDefault(ws => ws.Key.Contains("searchResultsV2"));
        if (searchResultsV2?.Value != null)
        {
            CategoryContent = JsonSerializer.Deserialize<CategoryContent>(searchResultsV2.Value.Value.ToString());
        }
        else if(LayoutItems != null)
        {
            CategoryContent = WidgetStates.Where(ws => LayoutItems.Any(l => l.StateId == ws.Key)).
                Select(ws => JsonSerializer.Deserialize<CategoryContent>(ws.Value.ToString()))
                .FirstOrDefault(c => !string.IsNullOrEmpty(c?.TileLayout));
        }

        if(CategoryContent == null)
        {
            var tileGridDesktop = WidgetStates?.FirstOrDefault(ws => ws.Key.Contains("tileGridDesktop"));
            if (tileGridDesktop != null)
                CategoryContent = JsonSerializer.Deserialize<CategoryContent>(tileGridDesktop.Value.Value.ToString());
        }


        if (!string.IsNullOrWhiteSpace(SharedContent))
        {
            Shared = JsonSerializer.Deserialize<Shared>(SharedContent);
        }
    }
}


public class CategoryCatalog
{
    [JsonPropertyName("totalFound")]
    public int TotalFound { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; }
}

public class Shared
{
    [JsonPropertyName("catalog")]
    public CategoryCatalog? Catalog { get; set; }
}
