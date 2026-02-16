using System.Text.Json.Serialization;

namespace Import.LoaderSettings;

public enum LoadingType
{
    Simple,
    Route,
    RequestFinished,
    Response,
    Api,
    WaitForUrl
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