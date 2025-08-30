using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Alchemist.BrowserService.IntegrationTest;

public class FirefoxBrowserServiceLaunchingTest : TestFixture<WebApplicationFactory<BrowserServiceProgramm>, BrowserServiceProgramm>
{
    private readonly HttpClient _httpClient;

    public FirefoxBrowserServiceLaunchingTest(WebApplicationFactory<BrowserServiceProgramm> webAppFactory, ITestOutputHelper outputHelper) : base(webAppFactory, outputHelper)
    {
        _httpClient = WebAppFactory.CreateClient();
    }

    [Fact]
    public async Task HelloResponseWhenStartingSuccess()
    {
        var response = await _httpClient.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello BrowserService!", hello);
    }

    [Fact]
    public async Task LaunchForExistingBrowserAndUrlSuccess()
    {
        var data = new ArrayList() { "firefox", "google.com" };
        var response = await _httpClient.PostAsJsonAsync("/browserdata/launch", data);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task LaunchForNotExistingBrowserAndUrlNotFound()
    {
        var data = new ArrayList() { Guid.NewGuid().ToString(), "google.com" };
        var response = await _httpClient.PostAsJsonAsync("/browserdata/launch", data);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }    
}