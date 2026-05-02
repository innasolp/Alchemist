using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Web;
using Xunit.Abstractions;

namespace Alchemist.BrowserService.IntegrationTest;

public class FirefoxBrowserServiceLaunchingTest : TestFixture<WebApplicationFactory<BrowserServiceProgramm>, BrowserServiceProgramm>
{
    private readonly HttpClient _httpClient;

    public FirefoxBrowserServiceLaunchingTest(WebApplicationFactory<BrowserServiceProgramm> webAppFactory, ITestOutputHelper outputHelper) : base(webAppFactory, outputHelper)
    {
        _httpClient = WebAppFactory.CreateClient();
    }

    private static string GetLaunchRequestUrl(string browser, string url)
    {
        var encodedBrowserLauncher = HttpUtility.UrlEncode(browser);
        var encodedHost = HttpUtility.UrlEncode(url);
        return $"browserdata/launch?browser={encodedBrowserLauncher}&url={encodedHost}";
    }    

    [Fact]
    public async Task LaunchForExistingBrowserAndUrlSuccess()
    {
        var requestUrl = GetLaunchRequestUrl("firefox", "google.com");
        var response = await _httpClient.PostAsync(requestUrl, null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task LaunchForNotExistingBrowserAndUrlNotFound()
    {
        var requestUrl = GetLaunchRequestUrl(Guid.NewGuid().ToString(), "google.com");
        var response = await _httpClient.PostAsync(requestUrl, null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }    
}