using BrowserDataLoader.Interfaces;
using Import.Interfaces;
using Import.Interfaces.Exceptions;
using Import.LoaderSettings;
using Moq;
using System.Text;
using WebLoader.Interfaces;
using RequestOptions = Import.LoaderSettings.RequestOptions;

namespace Alchemist.BrowserService.Client.UnitTest;

public class BrowserServiceClientWebLoaderCallsTest
{
    [Fact]
    public async Task LoadFromUrl_CallsWebLoaderLoadFromUrl_ReturnsStreamAsync()
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        webLoaderMock
            .Setup(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions?>()))
            .ReturnsAsync(expectedStream);

        var resultStream = await Helper.LoadAsync(webLoaderMock, "http://example", new object[] { Array.Empty<ICookieData>() }, cancellationToken: CancellationToken.None);

        Assert.Same(expectedStream, resultStream);
        webLoaderMock.Verify(w => w.LoadFromUrl("http://example", It.IsAny<WebLoader.Interfaces.RequestOptions?>()), Times.Once);
    }

    [Fact]
    public async Task LoadFromUrlWithRouteUrl_CallsWebLoaderTryLoadFromRoute_ReturnsStreamAsync()
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var url = "http://example";
        var routeUrl = "http://example/api";

        webLoaderMock
            .Setup(w => w.TryLoadFromRoute(url,
            It.Is<Func<string, bool>>(f => f.Invoke(routeUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, expectedStream));

        var loaderOptions = new RequestOptions { RouteUrlFormat = routeUrl };

        var resultStream = await Helper.LoadAsync(webLoaderMock, url, new object[] { loaderOptions }, cancellationToken: CancellationToken.None);

        Assert.Same(expectedStream, resultStream);
        webLoaderMock.Verify(w => w.TryLoadFromRoute(url,
            It.Is<Func<string, bool>>(f => f.Invoke(routeUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadFromInvalidRouteUrl_CallsWebLoaderTryLoadFromRoute_ThrowsExceptionRouteNotFoundAsync()
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var url = "http://example";
        var routeUrl = "http://example/api";
        var invalidRouteUrl = $"{Guid.NewGuid()}";        

        webLoaderMock
            .Setup(w => w.TryLoadFromRoute(url,
             It.Is<Func<string, bool>>(f => f.Invoke(routeUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, expectedStream));

        webLoaderMock
            .Setup(w => w.TryLoadFromRoute(url, It.Is<Func<string, bool>>(f => f.Invoke(invalidRouteUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, null));

        var loaderOptions = new RequestOptions { RouteUrlFormat = invalidRouteUrl };

        var ex = await Assert.ThrowsAnyAsync<LoaderServiceException>(async () =>
                await Helper.LoadAsync(webLoaderMock, url, new object[] { loaderOptions }, cancellationToken: CancellationToken.None));

        Assert.Contains($"Route {invalidRouteUrl} on page {url} not found", ex.Message);

        webLoaderMock.Verify(w => w.TryLoadFromRoute(url,
            It.Is<Func<string, bool>>(f => f.Invoke(invalidRouteUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadFromApi_CallsWebLoaderFromApi_ReturnsStreamAsync()
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var apiUrl = "http://example/api";

        var data = new { id = 1, name = "Name" };

        webLoaderMock
            .Setup(w => w.LoadFromApiUrl(apiUrl, HttpMethod.Post, data, It.IsAny<WebLoader.Interfaces.RequestOptions?>()))
            .ReturnsAsync(expectedStream);

        var loaderOptions = new RequestOptions { LoadingType = LoadingType.Api, HttpMethod = "POST", Data = data };

        var resultStream = await Helper.LoadAsync(webLoaderMock, apiUrl,
            new object[] { loaderOptions }, cancellationToken: CancellationToken.None);

        Assert.Same(expectedStream, resultStream);

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.LoadFromApiUrl(apiUrl, HttpMethod.Post, data, It.IsAny<WebLoader.Interfaces.RequestOptions?>()), Times.Once);
    }

    [Fact]
    public async Task LoadHostPageWithRouteUrl_CallsWebLoaderTryLoadFromUrl_ReturnsStreamAsync()
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var url = "http://example";
        var routeUrl = "http://example/api";

        webLoaderMock
            .Setup(w => w.TryLoadFromRoute(url,
            It.Is<Func<string, bool>>(f => f.Invoke(routeUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, expectedStream));

        var loaderOptions = new RequestOptions { RouteUrlFormat = routeUrl };

        var resultStream = await Helper.LoadHostAsync(webLoaderMock, url, hostRequestOptions: loaderOptions, cancellationToken: CancellationToken.None);

        Assert.Same(expectedStream, resultStream);

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.TryLoadFromRoute(url,
            It.Is<Func<string, bool>>(f => f.Invoke(routeUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadHostPageWithRouteUrl_WhenWebLoaderLoadFromRouteReturnFalse_ThrownLoaderServiceExceptionAsync()
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var url = "http://example";
        var routeUrl = "http://example/api";

        webLoaderMock
            .Setup(w => w.TryLoadFromRoute(url,
                It.Is<Func<string, bool>>(f => f.Invoke(routeUrl)),
                It.IsAny<WebLoader.Interfaces.RequestOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, expectedStream));

        var loaderOptions = new RequestOptions { RouteUrlFormat = routeUrl };

        await Assert.ThrowsAnyAsync<LoaderServiceException>
            (()=>Helper.LoadHostAsync(webLoaderMock, url, hostRequestOptions: loaderOptions, cancellationToken: CancellationToken.None));

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.TryLoadFromRoute(url, 
            It.Is<Func<string, bool>>(f => f.Invoke(routeUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadHostPageWithRouteUrl_CallsWebLoaderWaitForUrl_WhenLoadingTypeIsWaitForUrl_ReturnsStreamAsync()
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var url = "http://example";
        var routeUrl = "http://example/api";

        webLoaderMock
            .Setup(w => w.WaitForUrl(url,
            It.Is<Func<string, bool>>(f => f.Invoke(routeUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>()))
            .ReturnsAsync(expectedStream);

        var loaderOptions = new RequestOptions { RouteUrlFormat = routeUrl, LoadingType = LoadingType.WaitForUrl };

        var resultStream = await Helper.LoadHostAsync(webLoaderMock, url, hostRequestOptions: loaderOptions, cancellationToken: CancellationToken.None);

        Assert.Same(expectedStream, resultStream);

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.WaitForUrl(url, It.Is<Func<string, bool>>(f => f.Invoke(routeUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>()), Times.Once);
    }

}