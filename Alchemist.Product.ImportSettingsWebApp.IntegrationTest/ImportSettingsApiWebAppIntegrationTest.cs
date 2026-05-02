using Alchemist.Product.ImportSettingsWebApp.IntegrationTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.Product.ImportSettingsWebApp.IntegrationTest;

public class TestImportSettingsApiWebAppFactory : ImportSettingsWebAppLoggedFactory
{
    public TestImportSettingsApiWebAppFactory() : base(isApi: true,
    httpPort: 7084,
    httpsPort: 7085,
    settingsApiHttpPort: 8212,
    settingsApiHttpsPort: 8213,
    settingsDatabase: Alchemist.Common.ConfigurationHelper.GetSectionValue("SettingsWebApiTestDb"),
    shopDatabase: Alchemist.Common.ConfigurationHelper.GetSectionValue("ShopWebApiTestDb"))
    {
    }
}

public class ImportSettingsApiWebAppIntegrationTest : LoggedContextTestFixture<TestImportSettingsApiWebAppFactory, ImportSettingsWebAppProgramm>
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

        try
        {
            var httpClient = WebAppFactory.CreateClient();
            var response = await httpClient.GetAsync(swaggerUrl);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task HelloResponseSuccessAsync()
    {
        try
        {
            var httpClient = WebAppFactory.CreateClient();
            var response = await httpClient.GetAsync("/");
            response.EnsureSuccessStatusCode();
            var hello = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello ImportSettingsWebApp API!", hello);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task ImportSettingsApi_SettingsTabEndpoint_ReturnsShopsSettingsContent()
    {
        try
        {
            var response = await _client.PostAsync($"/ImportSettingsApi/Import/Settings/1/1", null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("importSettingsDiv", content);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }
}
