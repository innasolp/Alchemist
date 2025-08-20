using Alchemist.Import.Interfaces;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using Moq;

namespace Alchemist.Test.Product.Shop;

public abstract class ProductShopTest
{
    protected readonly Mock<IBrowserService> _browserServiceMock = new();

    protected abstract IBrowserDataLoader BrowserDataLoader { get;}

    protected abstract IBrowserLauncher BrowserLauncher { get; }

    public ProductShopTest()
    {
        _browserServiceMock.Setup(s => s.LoadCookies(It.IsAny<string>())).
            Returns(async (string host) => (await BrowserDataLoader.LoadCookies()).Select(c => c.Convert()));

        _browserServiceMock.Setup(s=>s.UpdateCookiesForUrl(It.IsAny<string>())).
            Returns(async (string url) => await BrowserLauncher.OpenUrl(url));
    }
}
