using Alchemist.Common;
using BrowserDataLoader.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Import.Html.Categories.Tests;

public static class HeadersHelper
{
    public static Dictionary<string, string> LoadHeadersForRequest(string filePath, IEnumerable<ICookieData> cookieValues)
    {
        var requestHeaders = filePath.ReadFromJsonFile<JsonObject>();

        requestHeaders.TryGetPropertyValue("Headers", out var headersJson);
        var headers = headersJson.Deserialize<Dictionary<string, string>>();

        requestHeaders.TryGetPropertyValue("Cookies", out var cookiesJson);
        var cookies = cookiesJson.Deserialize<string[]>();

        var cookiesDict = GetCookieValues(cookies, cookieValues, 0);
        var cookieString = string.Join("; ", cookiesDict.Select(c => $"{c.Key}={c.Value}"));
        headers.Add("Cookie", cookieString);

        return headers;
    }

    public static Dictionary<string,string> GetCookieValues(IEnumerable<string> cookieKeys, IEnumerable<ICookieData> allCookies, int cookiePrefixLength = 0)
    {
        return cookieKeys.Select(k =>
        {
            var cookieValue = allCookies.FirstOrDefault(c => c.Name == k);
            var bytes = cookieValue?.Value;
            var value = bytes != null ?
                     bytes.Length >= cookiePrefixLength ? Encoding.UTF8.GetString(bytes, cookiePrefixLength, bytes.Length - cookiePrefixLength)
                    : Encoding.UTF8.GetString(bytes)
                 : null;

            if (value == null)
            {
                var cookie = allCookies.FirstOrDefault(c => c.Name.EndsWith(k));
                if (cookie != null)
                    value = Encoding.UTF8.GetString(cookie.Value, cookiePrefixLength, cookie.Value.Length - cookiePrefixLength);
            }
            return new KeyValuePair<string, string>(k, value);
        }).ToDictionary();
    }    
}
