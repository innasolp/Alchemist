using BrowserDataLoader.Interfaces;
using Moq;

namespace Alchemist.BrowserService.UnitTest;

internal class BrowserDataLoaderMock : Mock<IBrowserDataLoader>, IBrowserDataLoader
{    
    public async Task<int> ClearCookiesForHost(string host, string[]? cookieKeys = null, CancellationToken cancellationToken = default)
    {
        return await Object.ClearCookiesForHost(host, cookieKeys, cancellationToken:cancellationToken);
    }

    public async Task<IEnumerable<ICookieData>> LoadCookies(string host, string[]? cookieKeys = null, CancellationToken cancellationToken = default,
        params object[] parameters)
    {
       return await Object.LoadCookies(host, cookieKeys, cancellationToken: cancellationToken, parameters);
    }
}

internal class BrowserDataLoaderMock1 : BrowserDataLoaderMock { }
internal class BrowserDataLoaderMock2 : BrowserDataLoaderMock { }
