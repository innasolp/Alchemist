using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.Ozon.Model;

public class WebRichDescription : IJsonOnDeserialized
{
    [JsonPropertyName("richAnnotationJson")]
    public RichAnnotationJson RichAnnotationJson { get; set; }

    [JsonIgnore]
    public string RichDescription { get; set; }

    [JsonPropertyName("richAnnotation")]
    public string RichAnnotation { get; set; }

    [JsonPropertyName("richAnnotationType")]
    public string RichAnnotationType { get; set; }

    public void OnDeserialized()
    {
        if (RichAnnotationJson != null)
        {
            var rows = RichAnnotationJson?.Contents?.SelectMany(c => c.FullText);
            RichDescription = string.Join(" ", rows);
        }
        else
            RichDescription = RichAnnotation;
    }
}

public class RichAnnotationJson : IJsonOnDeserialized
{
    [JsonPropertyName("content")]
    public RichAnnotationContent[]? Contents { get; set; }

    [JsonIgnore]
    public string? ProductTypeTitle { get; set; }

    public void OnDeserialized()
    {
        ProductTypeTitle = Contents?.Where(c=>c.Blocks != null).SelectMany(c=>c.Blocks)
                                   .FirstOrDefault(b => b?.RichAnnotationContentBlockType == RichAnnotationContentBlockType.Chess
                                                      && b.Title?.Content?.Length > 0)?.Title?.Content?.FirstOrDefault(c => !string.IsNullOrEmpty(c))?.Trim();
    }
}

public class RichAnnotationContent : IJsonOnDeserialized
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("blocks")]
    public RichAnnotationContentBlock[]? Blocks { get; set; }

    [JsonPropertyName("text")]
    public RichAnnotationContentBlockText? Text { get; set; }

    [JsonIgnore]
    public string FullText { get; set; }

    public void OnDeserialized()
    {
        FullText = string.Concat(
            Blocks != null ? string.Join(" ", Blocks.Where(b => b.Text != null && b.Text.Rows != null).SelectMany(b => b.Text.Rows)) : "",
            Text != null && Text.Rows != null ? string.Join(" ", Text.Rows) : ""
            );       
    }
}

public enum RichAnnotationContentBlockType
{
    Chess
}

public class RichAnnotationContentBlock : IJsonOnDeserialized
{
    private static readonly Dictionary<string, RichAnnotationContentBlockType> RichAnnotationContentBlockTypes;
    static RichAnnotationContentBlock()
    {
        RichAnnotationContentBlockTypes = Enum.GetValues(typeof(RichAnnotationContentBlockType)).Cast<RichAnnotationContentBlockType>().
            ToDictionary(k => k.ToString(), v => v);
    }

    [JsonPropertyName("text")]
    public RichAnnotationContentBlockText Text { get; set; }

    [JsonPropertyName("title")]
    public RichAnnotationContentBlockTitle? Title { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonIgnore]
    public RichAnnotationContentBlockType? RichAnnotationContentBlockType { get; set; }

    public void OnDeserialized()
    {
        var contentBlockType = RichAnnotationContentBlockTypes.FirstOrDefault(wct => wct.Key.ToLower() == Type?.Replace("_", ""));
        RichAnnotationContentBlockType = !string.IsNullOrEmpty(contentBlockType.Key) ?  contentBlockType.Value : null;
    }
}

public class RichAnnotationContentBlockText : IJsonOnDeserialized
{
    [JsonPropertyName("text")]
    public string[] TextRows { get; set; }

    [JsonPropertyName("content")]
    public string[] ContentRows { get; set; }

    [JsonIgnore]
    public string[]? Rows { get; set; }

    [JsonPropertyName("items")]
    public RichAnnotationContentBlockTextItem[] Items { get; set; }

    public void OnDeserialized()
    {
        Rows = TextRows ?? ContentRows ?? Items?.Where(i=>i.Type == "text").Select(i=>i.Content).ToArray();
    }
}

public class RichAnnotationContentBlockTitle
{
    [JsonPropertyName("content")]
    public string[]? Content { get; set; }
}

public class RichAnnotationContentBlockTextItem
{
    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}
