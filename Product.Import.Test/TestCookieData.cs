using BrowserDataLoader.Interfaces;

namespace Product.Import.Test;

internal class TestCookieData : ICookieData
{
    public string Name { get; set; }

    public byte[] Value { get; set; }

    public string Host { get; set; }
}

internal static class CookieDataExtensions
{
    public static TestCookieData Convert(this ICookieData cookieData)
    {
        return new TestCookieData
        {
            Host = cookieData.Host,
            Name = cookieData.Name,
            Value = cookieData.Value
        };
    }
}
