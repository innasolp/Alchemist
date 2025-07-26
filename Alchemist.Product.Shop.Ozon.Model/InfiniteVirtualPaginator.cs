using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public class InfiniteVirtualPaginator
{
    [JsonPropertyName("prevPage")]
    public string PrevPage { get; set; }

    [JsonPropertyName("nextPage")]
    public string NextPage { get; set; }

    [JsonPropertyName("layoutContainer")]
    public string LayoutContainer { get; set; }
}
