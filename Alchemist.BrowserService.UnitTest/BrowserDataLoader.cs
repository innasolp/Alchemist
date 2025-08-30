using BrowserDataLoader.Interfaces;
using Moq;

namespace Alchemist.BrowserService.UnitTest;

internal class BrowserDataLoaderMock : Mock<IBrowserDataLoader>, IBrowserDataLoader
{
    public async Task<int> ClearAllCookies()
    {
        return await Object.ClearAllCookies();
    }

    public async Task<int> ClearCookiesForHost(string host)
    {
        return await Object.ClearCookiesForHost(host);
    }

    public async Task<IEnumerable<ICookieData>> LoadCookies(string host, bool distinct = true)
    {
       return await Object.LoadCookies(host, distinct);
    }

    public async Task<IEnumerable<ICookieData>> LoadCookies()
    {
        return await Object.LoadCookies();
    }
}

internal class BrowserDataLoaderMock1 : BrowserDataLoaderMock { }
internal class BrowserDataLoaderMock2 : BrowserDataLoaderMock { }
