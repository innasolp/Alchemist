using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.ShopWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public class ImportSettingsShopClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner>(bool isApi,
    int httpPort,
    int httpsPort,
    TestServer signalRTestServer,
    int settingsApiHttpPort,
    int settingsApiHttpsPort,
    string settingsDbConnectionStringSection = "ConnectionStrings:DbContext",
    string settingsDatabase = "alchemy",
    string shopDbConnectionStringSection = "ConnectionStrings:DbContext",
    string shopDatabase = "alchemy",
    int? shopApiHttpPort = null,
    int? shopApiHttpsPort = null,
    int? shopWebappApiHttpPort = null,
    int? shopWebAppApiHttpsPort = null)
    : ImportSettingsShopClientWebAppFactory(isApi, httpPort, httpsPort, settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer, shopApiHttpPort, shopApiHttpsPort, shopWebappApiHttpPort, shopWebAppApiHttpsPort)
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
{
    protected override TestHostServerWebAppFactory<ShopWebAppProgram> CreateShopWebAppFactory(int shopWebappApiHttpPort, int shopWebAppApiHttpsPort, int shopApiHttpPort, int shopApiHttpsPort, TestServer signalRServer)
    {
        return ShopWebAppHelper.CreateShopApiClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner>(shopDbConnectionStringSection,
            shopDatabase,
            shopWebappApiHttpPort,
            shopWebAppApiHttpsPort,
            shopApiHttpPort,
            shopApiHttpsPort, signalRServer);
    }

    protected override TestHostServerWebAppFactory<SettingsAPIProgram> CreateSettingsApiWebAppFactory(int settingsApiHttpPort, int settingsApiHttpsPort, TestServer signalRServer)
    {
        return SettingsApiHelper.CreateSettingsApiWebAppFactory<TTestDbContainer, TDbRespawner>(settingsDbConnectionStringSection,
            settingsDatabase,
            settingsApiHttpPort,
            settingsApiHttpsPort,
            signalRServer);
    }
}