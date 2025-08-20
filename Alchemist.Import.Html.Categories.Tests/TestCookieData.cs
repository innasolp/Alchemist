namespace Alchemist.Import.Html.Categories.Tests;

internal class TestCookieData : WebLoader.Interfaces.ICookieData
{
    public string Name { get; set; }

    public byte[] Value { get; set; }

    public string Host { get; set; }
}

internal static class CookieDataExtensions
{
    public static TestCookieData Convert(this BrowserDataLoader.Interfaces.ICookieData cookieData)
    {
        return new TestCookieData
        {
            Host = cookieData.Host,
            Name = cookieData.Name,
            Value = cookieData.Value
        };
    }
}
