using BrowserDataLoader.Interfaces;

namespace Alchemist.BrowserService;

internal class CookieData(string name, byte[] value, string host) : ICookieData
{
    public string Name { get; } = name;

    public byte[] Value { get; } = value;

    public string Host { get; } = host;
}

public static class CookieDataExtensions
{
    public static ICookieData Convert(this ICookieData cookieData)
    {
        return new CookieData(cookieData.Name, cookieData.Value, cookieData.Host);
    }
}
