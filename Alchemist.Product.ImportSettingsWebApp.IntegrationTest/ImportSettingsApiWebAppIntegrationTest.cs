using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.Server.Fixtures;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.Product.ImportSettingsWebApp.IntegrationTest;

public class TestImportSettingsApiWebAppFactory : ImportsettingsWebAppFullFactory
{
    public TestImportSettingsApiWebAppFactory() : base(isApi: true,
       null,
    httpPort: 7084,
    httpsPort: 7085,
    settingsApiConnectionDbSection: "SettingsWebApiTestDb",
    settingsApiHttpPort: 8212,
    settingsApiHttpsPort: 8213,
    Common.SignalRTestServer)
    {       
    }
}

public class ImportSettingsApiWebAppIntegrationTest : TestFixture<TestImportSettingsApiWebAppFactory, ImportSettingsWebAppProgramm>
{
    private readonly HttpClient _client;

    public ImportSettingsApiWebAppIntegrationTest(TestImportSettingsApiWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
        : base(webAppFactory, outputHelper)
    {
        _client = WebAppFactory.CreateClient();
    }

    [Fact]
    public async Task SwaggerGenerationSuccessAsync()
    {
        var swaggerUrl = "/swagger/index.html";
        var httpClient = WebAppFactory.CreateClient();
        var response = await httpClient.GetAsync(swaggerUrl);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task HelloResponseSuccessAsync()
    {
        var httpClient = WebAppFactory.CreateClient();
        var response = await httpClient.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ImportSettingsWebApp API!", hello);
    }

    [Fact]
    public async Task ImportSettingsApi_SettingsTabEndpoint_ReturnsShopsSettingsContent()
    {
        var response = await _client.PostAsync($"/Tab/1/1", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("importSettingsDiv", content);
    }
}
