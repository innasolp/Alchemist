using Alchemist.BrowserService.Controllers;
using BrowserLauncher.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace Alchemist.BrowserService.UnitTest;

public class BrowserLaunchingControllerTest
{
    private readonly List<IBrowserLauncher> _browserLauncherMocks = [new BrowserLauncherMock1(), new BrowserLauncherMock2()];

    private readonly BrowserServiceController _browserServiceController;

    public BrowserLaunchingControllerTest()
    {
        _browserServiceController = new BrowserServiceController((new Mock<ILogger<BrowserServiceController>>()).Object, [], _browserLauncherMocks);
    }

    [Fact]
    public async Task LaunchBrowserBadRequestWhenBrowserAndUrlNotExists()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.LaunchBrowser([]));
        var badRequest = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("empty parameters list", badRequest.Value);
    }

    [Fact]
    public async Task LaunchBrowserBadRequestWhenSingleParameter()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.LaunchBrowser([""]));
        var badRequest = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("no second parameter", badRequest.Value);
    }

    [Fact]
    public async Task LaunchBrowserBadRequestWhenBrowserIsEmpty()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.LaunchBrowser(["", "url"]));
        var badRequest = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("browser is empty", badRequest.Value);
    }

    [Fact]
    public async Task LaunchBrowserBadRequestWhenUrlIsEmpty()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.LaunchBrowser(["browser", ""]));
        var badRequest = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("url is empty", badRequest.Value);
    }

    [Fact]
    public async Task LaunchBrowserNotFoundWhenBrowserNotExists()
    {
        var browser = Guid.NewGuid().ToString();
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.LaunchBrowser([browser, "url"]));
        var notFound = Assert.IsType<NotFound<string>>(result.Result);
        Assert.Equal(browser, notFound.Value);
    }

    [Fact]
    public async Task LaunchBrowserSuccessWhenBrowserAndUrlAreValid()
    {
        if (_browserLauncherMocks.FirstOrDefault(b => b is BrowserLauncherMock2) is not BrowserLauncherMock2 browserLauncher)
            throw new InvalidOperationException();

        browserLauncher.Setup(b => b.Launch(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.FromResult(true));

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _browserServiceController.LaunchBrowser([nameof(BrowserLauncherMock2), "url"]));
        Assert.IsType<Ok>(result.Result);
    }

}
