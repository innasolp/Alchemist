using BrowserDataLoader.Interfaces;

namespace Alchemist.BrowserService.IntegrationTest;

internal class CookieData : ICookieData
{
    public string Name { get; set; }

    public byte[] Value { get; set; }

    public string Host { get; set; }
}
