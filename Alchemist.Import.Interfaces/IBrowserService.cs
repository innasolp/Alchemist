namespace Alchemist.Import.Interfaces;

public interface ICookieData
{
    string Name { get; }

    byte[] Value { get; }

    string Host { get; }
}

public interface IBrowserService
{
    Task<IEnumerable<ICookieData>> LoadCookies(string host);

    Task UpdateCookiesForUrl(string url);
}
