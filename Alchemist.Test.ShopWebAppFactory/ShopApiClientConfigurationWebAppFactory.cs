using Alchemist.Product.Data;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.ShopApiFactory;
using Microsoft.AspNetCore.TestHost;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.ShopWebAppFactory;

public class ShopApiClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner>
    (bool isApi,
    int httpPort,
    int httpsPort, 
    int shopAPIHttpPort,
    int shopAPIHttpsPort, 
    TestServer signalRTestServer,
    string shopDbConnectionStringSection = "ConnectionStrings:DbContext",
    string shopDatabase = "alchemy",
    Action<AlchemyContext>? fillTestData = null) 
    : ShopApiClientWebAppFactory(isApi, httpPort, httpsPort, shopAPIHttpPort, shopAPIHttpsPort, signalRTestServer)
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
{
    protected override TestHostServerWebAppFactory<ShopAPIProgram> CreateShopApiFactory(int shopAPIHttpPort, int shopAPIHttpsPort, TestServer signalRTestServer)
    {
        return ShopApiHelper.CreateShopApiWebAppFactory<TTestDbContainer, TDbRespawner>(shopDbConnectionStringSection,
           shopDatabase,
           shopAPIHttpPort,
           shopAPIHttpsPort,
           signalRTestServer,
           fillTestData : fillTestData);
    }

    public Task ResetDatabaseAsync()
    {
        return ShopApiFactory is ShopApiConfigurationWebAppFactory<TTestDbContainer, TDbRespawner> shopApiContainerFactory
            ? shopApiContainerFactory.ResetDatabaseAsync()
            : Task.CompletedTask;
    }
}