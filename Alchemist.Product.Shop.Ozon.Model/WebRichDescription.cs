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

public class RichAnnotationJson
{
    [JsonPropertyName("content")]
    public RichAnnotationContent[] Contents { get; set; }
}

public class RichAnnotationContent : IJsonOnDeserialized
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("blocks")]
    public RichAnnotationContentBlock[] Blocks { get; set; }

    [JsonPropertyName("text")]
    public RichAnnotationContentBlockText Text { get; set; }

    [JsonIgnore]
    public string FullText { get; set; }

    public void OnDeserialized()
    {
        FullText = string.Concat(
            Blocks != null ? string.Join(" ", Blocks.Where(b => b.Text != null).SelectMany(b => b.Text.Rows)) : "",
            Text != null ? string.Join(" ", Text.Rows) : ""
            );
    }
}

public class RichAnnotationContentBlock
{
    [JsonPropertyName("text")]
    public RichAnnotationContentBlockText Text { get; set; }
}

public class RichAnnotationContentBlockText : IJsonOnDeserialized
{
    [JsonPropertyName("text")]
    public string[] TextRows { get; set; }

    [JsonPropertyName("content")]
    public string[] ContentRows { get; set; }

    [JsonIgnore]
    public string[] Rows { get; set; }

    public void OnDeserialized()
    {
        Rows = TextRows ?? ContentRows;
    }
}
