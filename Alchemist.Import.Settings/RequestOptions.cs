namespace Alchemist.Import.Settings;

public class RequestOptions
{
    public string? RouteUrlFormat { get; set; } = null;

    public int? TimeouteMillseconds { get; set; } = null;

    public Dictionary<string, object>? Parameters { get; set; } = null;
}