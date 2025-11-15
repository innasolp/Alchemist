using Alchemist.Product.Data;
using Alchemist.Test.SettingsAPIFactory;
using Microsoft.AspNetCore.TestHost;

namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public static class ImportSettingsWebAppHelper
{
    public static ImportSettingsWebAppFactory CreateImportSettingsWebAppApiFactory(string connectionStringSection,
        int httpPort,
        int httpsPort,
        string shopApiHost,
        int settingsApiHttpPort,
        int settingsApiHttpsPort,
        TestServer signalRTestServer)
    {
        var settingsApiClient = SettingsApiHelper.CreateSettingsApiHttpClient(connectionStringSection, settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer, true);
        return new ImportSettingsWebAppFactory(true, shopApiHost, httpPort, httpsPort, settingsApiClient);
    }

    public static HttpClient CreateImportSettingsWebAppApiHttpClient(string connectionStringSection,
        int httpPort,
        int httpsPort,
        string shopApiHost,
        int settingsApiHttpPort,
        int settingsApiHttpsPort,
        TestServer signalRTestServer)
    {
        var settingsApiClient = SettingsApiHelper.CreateSettingsApiHttpClient(connectionStringSection, settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer, true);
        var webAppFactory = new ImportSettingsWebAppFactory(true, shopApiHost, httpPort, httpsPort, settingsApiClient);
        var httpClient = webAppFactory.CreateClient();
        httpClient.BaseAddress = new Uri(webAppFactory.ServerAddress);
        return httpClient;
    }

    public static ImportSettingsWebAppFactory CreateImportSettingsWebAppApiFactory(string connectionStringSection,
        int httpPort,
        int httpsPort,
        string shopApiHost,
        int settingsApiHttpPort,
        int settingsApiHttpsPort,
        TestServer signalRTestServer,
        Action<AlchemyContext> fillTestData)
    {
        var settingsApiClient = SettingsApiHelper.CreateSettingsApiHttpClient(connectionStringSection, settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer, fillTestData, true);
        return new ImportSettingsWebAppFactory(true, shopApiHost, httpPort, httpsPort, settingsApiClient);
    }

    public static HttpClient CreateImportSettingsWebAppApiHttpClient(string connectionStringSection,
        int httpPort,
        int httpsPort,
        string shopApiHost,
        int settingsApiHttpPort,
        int settingsApiHttpsPort,
        TestServer signalRTestServer,
        Action<AlchemyContext> fillTestData
        )
    {
        var settingsApiClient = SettingsApiHelper.CreateSettingsApiHttpClient(connectionStringSection, settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer, fillTestData, true);
        var webAppFactory = new ImportSettingsWebAppFactory(true, shopApiHost, httpPort, httpsPort, settingsApiClient);
        var httpClient = webAppFactory.CreateClient();
        httpClient.BaseAddress = new Uri(webAppFactory.ServerAddress);
        return httpClient;
    }
}
