using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.BrowserService.IntegrationTest;

public class BrowserServiceStartTest(WebApplicationFactory<BrowserServiceProgramm> webAppFactory, ITestOutputHelper outputHelper)
    : TestFixture<WebApplicationFactory<BrowserServiceProgramm>, BrowserServiceProgramm>(webAppFactory, outputHelper)
{
    [Fact]
    public async Task HelloResponseWhenStartingSuccess()
    {
        var httpClient = WebAppFactory.CreateClient();
        var response = await httpClient.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello BrowserService!", hello);
    }
}