using Alchemist.Import.Settings;
using BrowserDataLoader.Interfaces;
using Import.Interfaces;
using Moq;
using System.Text;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client.UnitTest;

public class BrowserServiceClientWebLoaderCallsTest
{
    [Fact]
    public async Task LoadFromUrl_CallsWebLoaderLoadFromUrl_ReturnsStreamAsync()
    {
        // Arrange
        var webLoaderMock = new Mock<IWebLoader>();

        // Prepare a stream that will be returned by the mocked web loader.
        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        // Setup LoadFromUrl to return our stream.
        webLoaderMock
            .Setup(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions>()))
            .ReturnsAsync(expectedStream);

        var resultStream = await Helper.LoadAsync(webLoaderMock, "http://example", new object[] { Array.Empty<ICookieData>() }, cancellationToken: CancellationToken.None);

        // Assert
        Assert.Same(expectedStream, resultStream);

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.LoadFromUrl("http://example", It.IsAny<WebLoader.Interfaces.RequestOptions>()), Times.Once);
    }

    [Fact]
    public async Task LoadFromUrlWithRouteUrl_CallsWebLoaderTryLoadFromRoute_ReturnsStreamAsync()
    {
        // Arrange
        var webLoaderMock = new Mock<IWebLoader>();

        // Prepare a stream that will be returned by the mocked web loader.
        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var url = "http://example";
        var routeUrl = "http://example/api";

        // Setup LoadFromUrl to return our stream.
        webLoaderMock
            .Setup(w => w.TryLoadFromRoute(url, routeUrl, It.IsAny<WebLoader.Interfaces.RequestOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, expectedStream));

        var loaderOptions = new Import.Settings.RequestOptions { RouteUrlFormat = routeUrl };

        var resultStream = await Helper.LoadAsync(webLoaderMock, url, new object[] { loaderOptions }, cancellationToken: CancellationToken.None);

        // Assert
        Assert.Same(expectedStream, resultStream);

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.TryLoadFromRoute(url, routeUrl, It.IsAny<WebLoader.Interfaces.RequestOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadFromInvalidRouteUrl_CallsWebLoaderTryLoadFromRoute_ThrowsExceptionRouteNotFoundAsync()
    {
        // Arrange
        var webLoaderMock = new Mock<IWebLoader>();

        // Prepare a stream that will be returned by the mocked web loader.
        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var url = "http://example";
        var routeUrl = "http://example/api";

        // Setup LoadFromUrl to return our stream.
        webLoaderMock
            .Setup(w => w.TryLoadFromRoute(url, routeUrl, It.IsAny<WebLoader.Interfaces.RequestOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, expectedStream));

        webLoaderMock
            .Setup(w => w.TryLoadFromRoute(url, It.IsNotIn(routeUrl), It.IsAny<WebLoader.Interfaces.RequestOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, null));

        var invalidRouteUrl = $"{Guid.NewGuid()}";
        var loaderOptions = new Import.Settings.RequestOptions { RouteUrlFormat = invalidRouteUrl };

        var ex = await Assert.ThrowsAsync<LoaderServiceException>(async () =>
                await Helper.LoadAsync(webLoaderMock, url, new object[] { loaderOptions }, cancellationToken: CancellationToken.None));

        // Assert
        Assert.Contains($"Route {invalidRouteUrl} on page {url} not found", ex.Message);

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.TryLoadFromRoute(url, invalidRouteUrl, It.IsAny<WebLoader.Interfaces.RequestOptions?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadFromApi_CallsWebLoaderFromApi_ReturnsStreamAsync()
    {
        // Arrange
        var webLoaderMock = new Mock<IWebLoader>();

        // Prepare a stream that will be returned by the mocked web loader.
        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        //var url = "http://example";
        var apiUrl = "http://example/api";

        var data = new { id = 1, name = "Name" };

        // Setup LoadFromUrl to return our stream.  
        webLoaderMock
            .Setup(w => w.LoadFromApiUrl(apiUrl, HttpMethod.Post, data, It.IsAny<WebLoader.Interfaces.RequestOptions>()))
            .ReturnsAsync(expectedStream);

        var loaderOptions = new Import.Settings.RequestOptions { LoadingType = LoadingType.Api, HttpMethod = "POST", Data = data };

        var resultStream = await Helper.LoadAsync(webLoaderMock, apiUrl,
            new object[] { loaderOptions }, cancellationToken: CancellationToken.None);

        // Assert
        Assert.Same(expectedStream, resultStream);

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.LoadFromApiUrl(apiUrl, HttpMethod.Post, data, It.IsAny<WebLoader.Interfaces.RequestOptions>()), Times.Once);
    }

    [Fact]
    public async Task LoadHostPageWithRouteUrl_CallsWebLoaderWaitForUrl_ReturnsStreamAsync()
    {
        // Arrange
        var webLoaderMock = new Mock<IWebLoader>();

        // Prepare a stream that will be returned by the mocked web loader.
        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var url = "http://example";
        var routeUrl = "http://example/api";

        // Setup LoadFromUrl to return our stream.
        webLoaderMock
            .Setup(w => w.WaitForUrl(url, routeUrl, It.IsAny<WebLoader.Interfaces.RequestOptions?>()))
            .ReturnsAsync(expectedStream);

        var loaderOptions = new Import.Settings.RequestOptions { RouteUrlFormat = routeUrl };

        var resultStream = await Helper.LoadHostAsync(webLoaderMock, url, hostRequestOptions: loaderOptions, cancellationToken: CancellationToken.None);

        // Assert
        Assert.Same(expectedStream, resultStream);

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.WaitForUrl(url, routeUrl, It.IsAny<WebLoader.Interfaces.RequestOptions?>()), Times.Once);
    }
}