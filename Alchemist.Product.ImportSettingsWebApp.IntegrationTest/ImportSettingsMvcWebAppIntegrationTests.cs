using Alchemist.Product.ImportSettingsWebApp.IntegrationTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using ShopSettings.Interfaces;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.ImportSettingsWebApp.IntegrationTest;

public class TestImportSettingsMvcWebAppFactory : ImportSettingsWebAppLoggedFactory
{
    public TestImportSettingsMvcWebAppFactory() : base(isApi: false,
    httpPort: 7082,
    httpsPort: 7083,
    settingsApiHttpPort: 8210,
    settingsApiHttpsPort: 8211,
    settingsDatabase: Alchemist.Common.ConfigurationHelper.GetSectionValue("SettingsMvcTestDb"),
    shopDatabase: Alchemist.Common.ConfigurationHelper.GetSectionValue("ShopMvcTestDb"),
    shopApiHttpPort: 8070,
    shopApiHttpsPort: 8071,
    shopWebappApiHttpPort: 8410,
    shopWebAppApiHttpsPort: 8411)
    {
    }
}

public class ImportSettingsMvcWebAppIntegrationTests : LoggedContextTestFixture<TestImportSettingsMvcWebAppFactory, ImportSettingsWebAppProgramm>
{
    private readonly HttpClient _client;

    public ImportSettingsMvcWebAppIntegrationTests(TestImportSettingsMvcWebAppFactory factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
        _client = WebAppFactory.CreateClient();
    }

    [Fact]
    public async Task Get_Root_ReturnsSuccess()
    {
        try
        {
            var resp = await _client.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            var content = await resp.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task ImportSettings_Index_ShopsEndpoint_ReturnsShopsListContent()
    {
        var hrefFormat = $"/Import/Settings/{{0}}/{(int)ShopSettingType.Product}";

        try
        {
            var response = await _client.PostAsync("/ShopApi/ShopList", JsonContent.Create(new { HrefFormat = hrefFormat }));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("shop_item", content);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }
}