using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.ShopWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Test.DbContainer.Abstractions;
using Test.PostresqlTestContainer;

namespace Alchemist.Test.ProductWebAppFactory;

public class ProductAggregatorConfigurationWebAppFactory<TTestDbContainer, TDbRespawner> : ProductAggregatorWebAppFactory
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
{
    public ProductAggregatorConfigurationWebAppFactory(int httpPort, int httpsPort, 
        int shopApiHttpPort, int shopApiHttpsPort, 
        int shopWebAppApiHttpPort, int shopWebAppApiHttpsPort, 
        string shopDataBaseConnectionStringSection, string shopDatabase, int settingsApiHttpPort, int settingsApiHttpsPort, int settingsWebAppApiHttpPort, int settingsWebAppApiHttspPort, string settingsDataBaseConnectionStringSection, string settingsDatabase, TestServer signalRTestServer) : base(httpPort, httpsPort, shopApiHttpPort, shopApiHttpsPort, shopWebAppApiHttpPort, shopWebAppApiHttpsPort, shopDataBaseConnectionStringSection, shopDatabase, settingsApiHttpPort, settingsApiHttpsPort, settingsWebAppApiHttpPort, settingsWebAppApiHttspPort, settingsDataBaseConnectionStringSection, settingsDatabase, signalRTestServer)
    {        
    }

    protected override TestHostServerWebAppFactory<ImportSettingsWebAppProgramm> CreateSettingsWebAppFactory(int settingsApiHttpPort, int settingsApiHttpsPort, int settingsWebAppApiHttpPort, int settingsWebAppApiHttspPort, string settingsDataBaseConnectionStringSection, string settingsDatabase, TestServer signalRTestServer, int shopWebAppApiHttpPort, int shopWebAppApiHttspPort)
    {
        return new ImportSettingsShopClientConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner>(true,
            settingsWebAppApiHttpPort,
            settingsWebAppApiHttspPort,
            signalRTestServer,
            settingsApiHttpPort,
            settingsApiHttpsPort,
            settingsDataBaseConnectionStringSection,
            settingsDatabase,
            shopWebappApiHttpPort: shopWebAppApiHttpPort,
            shopWebAppApiHttpsPort: shopWebAppApiHttspPort
            );
    }

    protected override TestHostServerWebAppFactory<ShopWebAppProgram> CreateShopWebAppFactory(int shopApiHttpPort, int shopApiHttpsPort, int shopWebAppApiHttpPort, int shopWebAppApiHttspPort, TestServer signalRTestServer, string dataBaseConnectionStringSection, string database)
    {
        return ShopWebAppHelper.CreateShopApiClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner>(dataBaseConnectionStringSection,
            database,
            shopWebAppApiHttpPort,
            shopWebAppApiHttspPort,
            shopApiHttpPort,
            shopApiHttpsPort, signalRTestServer);
    }
}