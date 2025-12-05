using Alchemist.Import.Products.Interfaces;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Web;

namespace Alchemist.Product.Shop.Ozon.Model;

public class Category : IJsonOnDeserialized, ICategoryProducts, IPaginatorItem
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

    [JsonIgnore]
    public InfiniteVirtualPaginator? InfiniteVirtualPaginator { get; private set; }

    [JsonPropertyName("prevPage")]
    public string? PrevPage { get; set; }

    [JsonPropertyName("nextPage")]
    public string? NextPage { get; set; }

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

        if(string.IsNullOrEmpty(PrevPage) && string.IsNullOrEmpty(NextPage))
        {
            var infiniteVirtualPaginatorJson = WidgetStates?.FirstOrDefault(ws => ws.Key.Contains("InfiniteVirtualPaginator"));
            if(infiniteVirtualPaginatorJson?.Value != null)
            {
                InfiniteVirtualPaginator = JsonSerializer.Deserialize<InfiniteVirtualPaginator>(infiniteVirtualPaginatorJson.Value.Value.ToString());
            }
        }


        if (!string.IsNullOrWhiteSpace(SharedContent))
        {
            Shared = JsonSerializer.Deserialize<Shared>(SharedContent);
        }
    }
    
    string IPaginatorItem.GetPageUrl(string urlFormat, string item, int page)
    {
        return !string.IsNullOrEmpty(PrevPage)
            ? PrevPage
            : (!string.IsNullOrEmpty(InfiniteVirtualPaginator?.PrevPage)
                            ? InfiniteVirtualPaginator.PrevPage
                            : string.Format(urlFormat, item, page));
    }

    private static string GetUrlFormat(string urlFormat, string item, int page)
    {
        var uri = new Uri(string.Format(urlFormat, item, page));
        var baseUrl = uri.GetLeftPart(UriPartial.Path);
        NameValueCollection queryParameters = HttpUtility.ParseQueryString(uri.Query);
        if (queryParameters.Count == 0)
            return urlFormat;

        var urlParameter = queryParameters.GetKey(0);
        return $"{baseUrl}?{urlParameter}={{0}}";
    }

    string IPaginatorItem.GetNextPageUrl(string urlFormat,string item, int page)
    {
        var paginatorUrlFormat = GetUrlFormat(urlFormat, item, page);

        return !string.IsNullOrEmpty(NextPage)
             ? string.Format(paginatorUrlFormat, HttpUtility.UrlEncode(NextPage))
             : (!string.IsNullOrEmpty(InfiniteVirtualPaginator?.NextPage)
                             ? string.Format(paginatorUrlFormat, HttpUtility.UrlEncode(InfiniteVirtualPaginator.NextPage))
                             : string.Format(urlFormat, item, page + 1));
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