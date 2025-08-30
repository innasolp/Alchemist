using Alchemist.Import.Interfaces;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using Moq;

namespace Alchemist.Test.Product.Shop;

public abstract class ProductShopTest
{
    protected readonly Mock<ILoaderService> _browserServiceMock = new();

    protected abstract IBrowserDataLoader BrowserDataLoader { get;}

    protected abstract IBrowserLauncher BrowserLauncher { get; }

    public ProductShopTest()
    {
        _browserServiceMock.Setup(s => s.GetData(It.IsAny<string>())).
            Returns(async (string host) => await BrowserDataLoader.LoadCookies());

        _browserServiceMock.Setup(s=>s.UpdateData(It.IsAny<string>())).
            Returns(async (string url) => await BrowserLauncher.OpenUrl(url));
    }
}
