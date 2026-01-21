using Alchemist.Product.Data;
using Alchemist.Test.SettingsAPIFactory;
using Microsoft.AspNetCore.TestHost;

namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public static class ImportSettingsWebAppHelper
{
    public static HttpClient CreateImportSettingsWebAppApiHttpClient(string connectionString,
        int httpPort,
        int httpsPort,
        string shopApiHost,
        int settingsApiHttpPort,
        int settingsApiHttpsPort,
        TestServer signalRTestServer,
        bool ensureDeleted = false)
    {
        var settingsApiClient = SettingsApiHelper.CreateSettingsApiHttpClient(connectionString, settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer, ensureDeleted);
        var webAppFactory = new ImportSettingsWebAppFactory(true, shopApiHost, httpPort, httpsPort, settingsApiClient);
        var httpClient = webAppFactory.CreateClient();
        httpClient.BaseAddress = new Uri(webAppFactory.ServerAddress);
        return httpClient;
    }

    public static HttpClient CreateImportSettingsWebAppApiHttpClient(string connectionString,
        int httpPort,
        int httpsPort,
        string shopApiHost,
        int settingsApiHttpPort,
        int settingsApiHttpsPort,
        TestServer signalRTestServer,
        Action<AlchemyContext> fillTestData,
        bool ensureDeleted = false
        )
    {
        var settingsApiClient = SettingsApiHelper.CreateSettingsApiHttpClient(connectionString, settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer, fillTestData, ensureDeleted);
        var webAppFactory = new ImportSettingsWebAppFactory(true, shopApiHost, httpPort, httpsPort, settingsApiClient);
        var httpClient = webAppFactory.CreateClient();
        httpClient.BaseAddress = new Uri(webAppFactory.ServerAddress);
        return httpClient;
    }
}