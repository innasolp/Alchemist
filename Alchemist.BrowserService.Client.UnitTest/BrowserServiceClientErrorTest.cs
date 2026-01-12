using Import.Interfaces;
using Moq;
using System.Net;
using System.Text;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client.UnitTest;

public class BrowserServiceClientErrorTest
{
    [Fact]
    public async Task LoadFromUrl_ThrowsException_WhenDataIsNotIEnumerableAsync()
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

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async ()=> 
                await Helper.LoadAsync(webLoaderMock, "http://example", new object(), cancellationToken : CancellationToken.None));

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions>()), Times.Never);
        Assert.Contains("Invalid type", exception.Message);
    }

    [Fact]
    public async Task LoadFromUrl_ThrowsLoaderServiceException_WhenHttpRequestErrorAsync()
    {
        // Arrange
        var webLoaderMock = new Mock<IWebLoader>();

        // Prepare a stream that will be returned by the mocked web loader.
        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var httpRequestException = new HttpRequestException(HttpRequestError.InvalidResponse, "service error", statusCode: HttpStatusCode.InternalServerError);
        var url = "http://example";

        // Setup LoadFromUrl to return our stream.
        webLoaderMock
            .Setup(w => w.LoadFromUrl(url, It.IsAny<WebLoader.Interfaces.RequestOptions>()))
            .Throws(httpRequestException);       

        var errorMessage = $"Request error {httpRequestException.HttpRequestError}, status code {httpRequestException.StatusCode}. {httpRequestException.Message}";
        var resultException = await Assert.ThrowsAsync<LoaderServiceException>(async ()=>
                await Helper.LoadAsync(webLoaderMock, url, Array.Empty<object>(), cancellationToken: CancellationToken.None));

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions>()), Times.Once);
        Assert.Contains(errorMessage, resultException.Message);
        Assert.Null(resultException.NeedAction);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task LoadFromUrl_ThrowsLoaderServiceExceptionWithWaitActionNeed_WhenAccessHttpRequestErrorAsync(HttpStatusCode httpStatusCode)
    {
        // Arrange
        var webLoaderMock = new Mock<IWebLoader>();

        // Prepare a stream that will be returned by the mocked web loader.
        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var httpRequestException = new HttpRequestException(HttpRequestError.InvalidResponse, "service error", statusCode: httpStatusCode);
        var url = "http://example";

        // Setup LoadFromUrl to return our stream.
        webLoaderMock
            .Setup(w => w.LoadFromUrl(url, It.IsAny<WebLoader.Interfaces.RequestOptions>()))
            .Throws(httpRequestException);

        var errorMessage = $"Request error {httpRequestException.HttpRequestError}, status code {httpRequestException.StatusCode}. {httpRequestException.Message}";
        var resultException = await Assert.ThrowsAsync<LoaderServiceException>(async () =>
                await Helper.LoadAsync(webLoaderMock, url, Array.Empty<object>(), cancellationToken: CancellationToken.None));

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions>()), Times.Once);
        Assert.Contains(errorMessage, resultException.Message);
        Assert.Equal(LoaderServiceAction.Wait, resultException.NeedAction);
    }

    [Fact]
    public async Task LoadFromUrl_ThrowsLoaderServiceExceptionWithResetNeeded_WhenNsredirectLoopErrorAsync()
    {
        // Arrange
        var webLoaderMock = new Mock<IWebLoader>();

        // Prepare a stream that will be returned by the mocked web loader.
        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var webloaderException = new WebLoaderException(NsError.NS_ERROR_REDIRECT_LOOP, "service error");
        var url = "http://example";

        // Setup LoadFromUrl to return our stream.
        webLoaderMock
            .Setup(w => w.LoadFromUrl(url, It.IsAny<WebLoader.Interfaces.RequestOptions>()))
            .Throws(webloaderException);

        var resultException = await Assert.ThrowsAsync<LoaderServiceException>(async () =>
                await Helper.LoadAsync(webLoaderMock, url, Array.Empty<object>(), cancellationToken: CancellationToken.None));

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions>()), Times.Once);
        Assert.Contains(webloaderException.Message, resultException.Message);
        Assert.Equal(LoaderServiceAction.Reset,  resultException.NeedAction);
    }

    [Fact]
    public async Task LoadFromForbiddenRouteUrl_CallsWebLoaderTryLoadFromRoute_ThrowsExceptionRouteFailedAsync()
    {
        // Arrange
        var webLoaderMock = new Mock<IWebLoader>();

        // Prepare a stream that will be returned by the mocked web loader.
        var errorMessage = "403 error";
        var expectedBytes = Encoding.UTF8.GetBytes(errorMessage);
        var expectedStream = new MemoryStream(expectedBytes);

        var url = "http://example";
        var routeUrl = "http://example/api";

        // Setup LoadFromUrl to return our stream.
        webLoaderMock
            .Setup(w => w.TryLoadFromRoute(url, routeUrl, It.IsAny<WebLoader.Interfaces.RequestOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, expectedStream));        

        var loaderOptions = new Import.Settings.RequestOptions { RouteUrlFormat = routeUrl };

        var ex = await Assert.ThrowsAsync<LoaderServiceException>(async () => 
                await Helper.LoadAsync(webLoaderMock, url, new object[] { loaderOptions }, cancellationToken: CancellationToken.None));

        // Assert
        Assert.Contains($"Route {routeUrl} on page {url} failed. {errorMessage}", ex.Message);

        webLoaderMock.Verify(w => w.Start(), Times.Once);
        webLoaderMock.Verify(w => w.TryLoadFromRoute(url, routeUrl, It.IsAny<WebLoader.Interfaces.RequestOptions?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}