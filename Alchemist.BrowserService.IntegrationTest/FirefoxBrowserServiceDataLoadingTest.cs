using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using Xunit.Abstractions;


namespace Alchemist.BrowserService.IntegrationTest;

public class FirefoxBrowserServiceDataLoadingTest : TestFixture<WebApplicationFactory<BrowserServiceProgramm>, BrowserServiceProgramm>, IAsyncLifetime
{
    private readonly HttpClient _httpClient;

    public FirefoxBrowserServiceDataLoadingTest(WebApplicationFactory<BrowserServiceProgramm> webAppFactory, ITestOutputHelper outputHelper)
        : base(webAppFactory, outputHelper)
    {
        _httpClient = WebAppFactory.CreateClient();
    }

    private async Task Launch(string browser, string url)
    {
        var data = new ArrayList() { browser, url };
        var response = await _httpClient.PostAsJsonAsync("/browserdata/launch", data);
        response.EnsureSuccessStatusCode();
    }

    public async Task InitializeAsync()
    {
        await Launch("firefox", "www.google.com");
    }

    private async Task Pause() => await Task.Delay(1000);

    [Fact]
    public async Task LoadCookiesFromExistingHostSuccess()
    {
        var response = await _httpClient.GetAsync("/browserdata/getCookies/firefox/www.google.com");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await Pause();
    }

    [Fact]
    public async Task CookiesFromExistingHostNotEmpty()
    {
        var response = await _httpClient.GetAsync("/browserdata/getCookies/firefox/www.google.com");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cookies = await response.Content.ReadFromJsonAsync<List<CookieData>>();
        Assert.NotEmpty(cookies);
        await Pause();
    }

    [Fact]
    public async Task LoadCookiesFoNonExistingBrowserNotFound()
    {
        var browser = Guid.NewGuid().ToString();
        var response = await _httpClient.GetAsync($"/browserdata/getCookies/{browser}/google.com");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await Pause();
    }

    [Fact]
    public async Task ClearCookiesForHostForNotExistingBrowserNotFound()
    {
        var browser = Guid.NewGuid().ToString();
        var encodedBrowserDataLoader = HttpUtility.UrlEncode(browser);
        var encodedHost = HttpUtility.UrlEncode("google.com");
        var url = $"browserdata/clearCookies?browser={encodedBrowserDataLoader}&host={encodedHost}";

        var response = await _httpClient.PostAsync(url, null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await Pause();
    }

    [Fact]
    public async Task ClearCookiesForHostForExistingBrowserSuccess()
    {
        var encodedBrowserDataLoader = HttpUtility.UrlEncode("firefox");
        var encodedHost = HttpUtility.UrlEncode("google.com");
        var url = $"browserdata/clearCookies?browser={encodedBrowserDataLoader}&host={encodedHost}";

        var response = await _httpClient.PostAsync(url, null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        Assert.True(int.TryParse(result, out var deleted));

        await Pause();
    }

    public async Task DisposeAsync()
    {
        await Task.FromResult(true);
    }
}
