using BrowserDataLoader.Interfaces;
using Moq;

namespace Alchemist.BrowserService.UnitTest;

internal class BrowserDataLoaderMock : Mock<IBrowserDataLoader>, IBrowserDataLoader
{
    public bool CheckStatus()
    {
        return Object.CheckStatus();
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
