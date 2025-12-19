using System.Text.Json.Serialization;

namespace Alchemist.BrowserService.Client;

internal class RequestHeaders 
{
    [JsonPropertyName("Headers")]
    public Dictionary<string, string> Headers { get; set; }


    [JsonPropertyName("Cookies")]
    public IList<string> CookieKeys { get; set; }


    [JsonPropertyName("CookieIndex")]
    public int CookieHeaderIndex { get; set; }


    [JsonPropertyName("CookiePrefixLength")]
    public int CookiePrefixLength { get; set; }


    [JsonPropertyName("DefaultCookies")]
    public Dictionary<string, string> DefaultCookieValues { get; set; }

    public string CookieHeaderKey { get; } = "cookie"; 
}
