using Alchemist.BrowserService.Controllers;
using BrowserDataLoader.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace Alchemist.BrowserService.UnitTest;

public class BrowserDataLoadingControllerTest
{
    private readonly List<IBrowserDataLoader> _browserDataLoaders = [new BrowserDataLoaderMock2(), new BrowserDataLoaderMock1()];

    private readonly BrowserServiceController _browserServiceController;

    public BrowserDataLoadingControllerTest()
    {
        _browserServiceController = new BrowserServiceController(new Mock<ILogger<BrowserServiceController>>().Object, _browserDataLoaders, []);
    }

    [Fact]
    public async Task GetCookiesBadRequestWhenBrowserIsEmpty()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.GetCookies("", ""));
        var badRequest = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("browser is empty", badRequest.Value);
    }

    [Fact]
    public async Task GetCookiesBadRequestWhenUrlIsEmpty()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.GetCookies("browser", ""));
        var badRequest = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("host is empty", badRequest.Value);
    }

    [Fact]
    public async Task GetCookiesNotFoundWhenBrowserNotExists()
    {
        var browser = Guid.NewGuid().ToString();
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.GetCookies(browser, "url"));
        var notFound = Assert.IsType<NotFound<string>>(result.Result);
        Assert.Equal(browser, notFound.Value);
    }

    [Fact]
    public async Task GetCookiesNotFoundWhenBrowserCookiesEmpty()
    {
        if (_browserDataLoaders.FirstOrDefault(b => b is BrowserDataLoaderMock2) is not BrowserDataLoaderMock2 browserDataLoader)
            throw new InvalidOperationException();

        browserDataLoader.Setup(b => b.LoadCookies()).Returns(Task.FromResult((IEnumerable<ICookieData>)[]));

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.GetCookies(nameof(BrowserDataLoaderMock2), "url"));
        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task GetCookiesSuccessWhenBrowserCookiesNotEmpty()
    {
        if (_browserDataLoaders.FirstOrDefault(b => b is BrowserDataLoaderMock1) is not BrowserDataLoaderMock1 browserDataLoader)
            throw new InvalidOperationException();

        browserDataLoader.Setup(b => b.LoadCookies(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult((IEnumerable<ICookieData>) [
            new Mock<ICookieData>().Object,
            new Mock<ICookieData>().Object]));

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.GetCookies(nameof(BrowserDataLoaderMock1), "url"));
        var okResult = Assert.IsAssignableFrom<Ok<IEnumerable<ICookieData>>>(result.Result);
        Assert.Equal(2, okResult.Value?.Count());
    }

    [Fact]
    public async Task ClearCookiesForHostBadRequestWhenBrowserIsEmpty()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.ClearCookiesForHost("", ""));
        var badRequest = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("browser is empty", badRequest.Value);
    }

    [Fact]
    public async Task ClearCookiesForHostBadRequestWhenUrlIsEmpty()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.ClearCookiesForHost("browser", ""));
        var badRequest = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("host is empty", badRequest.Value);
    }

    [Fact]
    public async Task ClearCookiesForHostNotFoundWhenBrowserNotExists()
    {
        var browser = Guid.NewGuid().ToString();
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.ClearCookiesForHost(browser, "url"));
        var notFound = Assert.IsType<NotFound<string>>(result.Result);
        Assert.Equal(browser, notFound.Value);
    }

    [Fact]
    public async Task ClearCookiesForHostSuccessWhenBrowserAndUrlAreValid()
    {
        if (_browserDataLoaders.FirstOrDefault(b => b is BrowserDataLoaderMock1) is not BrowserDataLoaderMock1 browserDataLoader)
            throw new InvalidOperationException();

        var cookieCount = new Random().Next();
        browserDataLoader.Setup(b => b.ClearCookiesForHost("url")).Returns(Task.FromResult(cookieCount));

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.ClearCookiesForHost(nameof(BrowserDataLoaderMock1), "url"));
        var okResult = Assert.IsAssignableFrom<Ok<int>>(result.Result);
        Assert.Equal(cookieCount, okResult.Value);
    }
}