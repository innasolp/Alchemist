using BrowserDataLoader.Interfaces;
using Moq;

namespace Alchemist.BrowserService.UnitTest;

internal class BrowserDataLoaderMock : Mock<IBrowserDataLoader>, IBrowserDataLoader
{    
    public async Task<int> ClearCookiesForHost(string host, CancellationToken cancellationToken = default)
    {
        return await Object.ClearCookiesForHost(host, cancellationToken);
    }

    public async Task<IEnumerable<ICookieData>> LoadCookies(string host, bool distinct = true, CancellationToken cancellationToken = default,
        params object[] parameters)
    {
       return await Object.LoadCookies(host, distinct, cancellationToken, parameters);
    }
}

internal class BrowserDataLoaderMock1 : BrowserDataLoaderMock { }
internal class BrowserDataLoaderMock2 : BrowserDataLoaderMock { }
