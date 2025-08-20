using Alchemist.Import.Interfaces;

namespace Alchemist.BrowserService.Client;

internal class CookieData : ICookieData
{
    public string Name { get; set; }

    public byte[] Value { get; set; }

    public string Host { get; set; }
}
