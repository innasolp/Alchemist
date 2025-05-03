using Alchemist.Test.Server.Fixtures;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceTest : TestFixture<ImportBackgroundServiceWebAppFactory, ImportBackgroundServiceProgram>
{
    public ImportBackgroundServiceTest(ImportBackgroundServiceWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : base(webAppFactory, outputHelper)
    {
        WebAppFactory.CreateClient();   
    }

    [Fact]
    public async Task Start()
    {
        var httpClient = HttpClientFactory.Create();
        var response = await httpClient.GetAsync("http://localhost:8130/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ImportBackgroundService!", hello);
    }
}