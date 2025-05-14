using Alchemist.Test.Server.Fixtures;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceTest : TestFixture<ImportBackgroundServiceWebAppFactory, ImportBackgroundServiceProgram>
{
    private readonly HttpClient _httpClient;
    public ImportBackgroundServiceTest(ImportBackgroundServiceWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : base(webAppFactory, outputHelper)
    {
        _httpClient = WebAppFactory.CreateClient();   
    }

    [Fact]
    public async Task Start()
    {
        var response = await _httpClient.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ImportBackgroundService!", hello);
    }
}