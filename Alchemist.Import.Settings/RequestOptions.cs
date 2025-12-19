using System.Text.Json.Nodes;

namespace Alchemist.Import.Settings;

public class RequestOptions
{
    public string? HttpMethod { get; set; } = "GET";

    public JsonObject? Data { get; set; }

    public string? ApiUrlFormat { get; set; }
}