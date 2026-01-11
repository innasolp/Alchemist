using System.Text.Json.Serialization;

namespace Alchemist.Import.Settings;

public enum LoadingType
{
    Simple,
    Route,
    Request,
    Api
}

public class RequestOptions
{
    public string? RouteUrlFormat { get; set; } = null;

    public int? TimeouteMillseconds { get; set; } = null;

    public Dictionary<string, object>? Parameters { get; set; } = null;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LoadingType? LoadingType { get; set; } = null;

    public string? HttpMethod { get; set; } = null;

    public object? Data { get; set; } = null;
}