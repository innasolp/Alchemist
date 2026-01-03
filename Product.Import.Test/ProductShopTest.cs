using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using Import.Interfaces;
using Moq;

namespace Product.Import.Test;

public abstract class ProductShopTest
{
    protected readonly Mock<ILoaderService> _browserServiceMock = new();

    protected abstract IBrowserDataLoader BrowserDataLoader { get;}

    protected abstract IBrowserLauncher BrowserLauncher { get; }

    public ProductShopTest()
    {
        _browserServiceMock.Setup(s => s.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>())).
            Returns(async (string host, CancellationToken token) => await BrowserDataLoader.LoadCookies(host));

        _browserServiceMock.Setup(s=>s.UpdateData(It.IsAny<string>(), It.IsAny<CancellationToken>())).
            Returns(async (string url, CancellationToken token) => await BrowserLauncher.OpenUrl(url));
    }
}