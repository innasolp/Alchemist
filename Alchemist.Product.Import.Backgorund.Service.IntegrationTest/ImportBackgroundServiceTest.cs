using Alchemist.Test.Server.Fixtures;
using System.Net;
using Xunit.Abstractions;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

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
        var httpClient = WebAppFactory.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
        var response = await httpClient.GetAsync("http://localhost:8130/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ImportBackgroundService!", hello);
    }
}