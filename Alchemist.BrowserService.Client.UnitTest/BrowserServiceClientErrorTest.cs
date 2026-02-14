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
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        webLoaderMock
            .Setup(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions>()))
            .ReturnsAsync(expectedStream);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async ()=> 
                await Helper.LoadAsync(webLoaderMock, "http://example", new object(), cancellationToken : CancellationToken.None));

        webLoaderMock.Verify(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions>()), Times.Never);
        Assert.Contains("Invalid type", exception.Message);
    }

    [Fact]
    public async Task LoadFromUrl_ThrowsLoaderServiceException_WhenHttpRequestErrorAsync()
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var httpRequestException = new HttpRequestException(HttpRequestError.InvalidResponse, "service error", statusCode: HttpStatusCode.InternalServerError);
        var url = "http://example";

        webLoaderMock
            .Setup(w => w.LoadFromUrl(url, It.IsAny<WebLoader.Interfaces.RequestOptions>()))
            .Throws(httpRequestException);       

        var errorMessage = $"Request error {httpRequestException.HttpRequestError}, status code {httpRequestException.StatusCode}. {httpRequestException.Message}";
        var resultException = await Assert.ThrowsAsync<LoaderServiceException>(async ()=>
                await Helper.LoadAsync(webLoaderMock, url, Array.Empty<object>(), cancellationToken: CancellationToken.None));

        webLoaderMock.Verify(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions>()), Times.Once);
        Assert.Contains(errorMessage, resultException.Message);
        Assert.Null(resultException.NeedAction);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task LoadFromUrl_ThrowsLoaderServiceExceptionWithWaitActionNeed_WhenAccessHttpRequestErrorAsync(HttpStatusCode httpStatusCode)
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var httpRequestException = new HttpRequestException(HttpRequestError.InvalidResponse, "service error", statusCode: httpStatusCode);
        var url = "http://example";

        webLoaderMock
            .Setup(w => w.LoadFromUrl(url, It.IsAny<WebLoader.Interfaces.RequestOptions>()))
            .Throws(httpRequestException);

        var errorMessage = $"Request error {httpRequestException.HttpRequestError}, status code {httpRequestException.StatusCode}. {httpRequestException.Message}";
        var resultException = await Assert.ThrowsAsync<LoaderServiceException>(async () =>
                await Helper.LoadAsync(webLoaderMock, url, Array.Empty<object>(), cancellationToken: CancellationToken.None));

        webLoaderMock.Verify(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions>()), Times.Once);
        Assert.Contains(errorMessage, resultException.Message);
        Assert.Equal(LoaderServiceAction.Wait, resultException.NeedAction);
    }

    [Fact]
    public async Task LoadFromUrl_ThrowsLoaderServiceExceptionWithResetNeeded_WhenNsredirectLoopErrorAsync()
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var expectedBytes = Encoding.UTF8.GetBytes("hello world");
        var expectedStream = new MemoryStream(expectedBytes);

        var webloaderException = new WebLoaderException(NsError.NS_ERROR_REDIRECT_LOOP, "service error");
        var url = "http://example";

        webLoaderMock
            .Setup(w => w.LoadFromUrl(url, It.IsAny<WebLoader.Interfaces.RequestOptions>()))
            .Throws(webloaderException);

        var resultException = await Assert.ThrowsAsync<LoaderServiceException>(async () =>
                await Helper.LoadAsync(webLoaderMock, url, Array.Empty<object>(), cancellationToken: CancellationToken.None));

        webLoaderMock.Verify(w => w.LoadFromUrl(It.IsAny<string>(), It.IsAny<WebLoader.Interfaces.RequestOptions>()), Times.Once);
        Assert.Contains(webloaderException.Message, resultException.Message);
        Assert.Equal(LoaderServiceAction.Reset,  resultException.NeedAction);
    }

    [Fact]
    public async Task LoadFromForbiddenRouteUrl_CallsWebLoaderTryLoadFromRoute_ThrowsExceptionRouteFailedAsync()
    {
        var webLoaderMock = new Mock<IWebLoader>();

        var errorMessage = "403 error";
        var expectedBytes = Encoding.UTF8.GetBytes(errorMessage);
        var expectedStream = new MemoryStream(expectedBytes);

        var url = "http://example";
        var routeUrl = "http://example/api";
        
        webLoaderMock
            .Setup(w => w.TryLoadFromRoute(url, It.Is<Func<string, bool>>(f => f.Invoke(routeUrl)),
                    It.IsAny<WebLoader.Interfaces.RequestOptions?>(), 
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, expectedStream));        

        var loaderOptions = new Import.Settings.RequestOptions { RouteUrlFormat = routeUrl };

        var ex = await Assert.ThrowsAsync<LoaderServiceException>(async () => 
                await Helper.LoadAsync(webLoaderMock, url, new object[] { loaderOptions }, cancellationToken: CancellationToken.None));

        Assert.Contains($"Route {routeUrl} on page {url} failed. {errorMessage}", ex.Message);

        webLoaderMock.Verify(w => w.TryLoadFromRoute(url, 
            It.Is<Func<string, bool>>(f=>f.Invoke(routeUrl)),
            It.IsAny<WebLoader.Interfaces.RequestOptions?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}