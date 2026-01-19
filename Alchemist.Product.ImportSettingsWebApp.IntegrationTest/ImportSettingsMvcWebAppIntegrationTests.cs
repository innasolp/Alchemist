using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.ShopWebAppFactory;
using ShopSettings.Interfaces;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.ImportSettingsWebApp.IntegrationTest;

public class TestImportSettingsMvcWebAppFactory : ImportsettingsWebAppFullFactory
{
    private readonly ShopWebAppFactory _shopWebAppFactory;

    public TestImportSettingsMvcWebAppFactory() : base(isApi : false,
        "https://localhost:8411",
    httpPort : 7082,
    httpsPort : 7083,
    settingsApiDbConnectionString: Alchemist.Common.ConfigurationHelper.GetConnectionString("SettingsMvcTestDb"),
    settingsApiHttpPort : 8210,
    settingsApiHttpsPort : 8211,
    Common.SignalRTestServer)
    {
        _shopWebAppFactory = new ShopWebAppFullFactory(isApi: true,
            connectionString: Alchemist.Common.ConfigurationHelper.GetConnectionString("SettingsMvcTestDb"),
            httpPort: 8410, httpsPort: 8411, shopAPIHttpPort: 8070, shopAPIHttpsPort: 8071,
            Common.SignalRTestServer
            );
        _shopWebAppFactory.CreateClient();
    }
}

public class ImportSettingsMvcWebAppIntegrationTests : TestFixture<TestImportSettingsMvcWebAppFactory, ImportSettingsWebAppProgramm>
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
        var resp = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var content = await resp.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(content));
    }

    [Fact]
    public async Task ImportSettings_Index_ShopsEndpoint_ReturnsShopsListContent()
    {
        var hrefFormat = $"/Import/Settings/{{0}}/{(int)ShopSettingType.Product}";
        var response = await _client.PostAsync("/ShopApi/ShopList", JsonContent.Create(new { HrefFormat = hrefFormat }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("shop_item", content);
    }
}