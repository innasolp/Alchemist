using Alchemist.Product.Data;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.ShopWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Test.DbContainer.Abstractions;
using Xunit;

namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public class ImportSettingsShopClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(bool isApi,
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
    int? shopWebAppApiHttpsPort = null,
    Action<AlchemyContext>? fillShopTestData = null,
    Action<AlchemyContext>? fillSettingsTestData = null)
    : ImportSettingsShopClientWebAppFactory(isApi,
        httpPort, httpsPort, 
        settingsApiHttpPort, settingsApiHttpsPort, 
        signalRTestServer,
        shopApiHttpPort, shopApiHttpsPort, 
        shopWebappApiHttpPort, shopWebAppApiHttpsPort,
        fillShopTestData,
        fillSettingsTestData)
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
    where TDbChecker : class, IDbChecker, new()
{
    private TestHostServerWebAppFactory<SettingsAPIProgram>? _settingsApiFactory;

    private TestHostServerWebAppFactory<ShopWebAppProgram>? _shopWebAppFactory;

    protected override async Task LifetimeDisposeAsync()
    {
        if (_shopWebAppFactory is IAsyncLifetime asyncLifetimeShopFactory)
            await asyncLifetimeShopFactory.DisposeAsync();

        if (_settingsApiFactory is IAsyncLifetime asyncLifetimeSettingsFactory)
            await asyncLifetimeSettingsFactory.DisposeAsync();

        await base.LifetimeDisposeAsync();
    }

    protected override async Task<HttpClient?> CreateShopWebAppHttpClientAsync(int? shopWebappApiHttpPort, 
        int? shopWebAppApiHttpsPort,
        int? shopApiHttpPort, int? shopApiHttpsPort, TestServer signalRServer, Action<AlchemyContext>? fillTestData = null)
    {
        var shopPorts = new int?[] { shopWebappApiHttpPort, shopWebAppApiHttpsPort, shopApiHttpPort, shopApiHttpsPort };
        if (shopPorts.All(p => p.HasValue))
        {
            _shopWebAppFactory = ShopWebAppHelper.CreateShopApiClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(shopDbConnectionStringSection,
            shopDatabase,
            shopWebappApiHttpPort!.Value,
            shopWebAppApiHttpsPort!.Value,
            shopApiHttpPort!.Value,
            shopApiHttpsPort!.Value, signalRServer, fillTestData);

            if (_shopWebAppFactory is IAsyncLifetime asyncLifetimeShopFactory)
                await asyncLifetimeShopFactory.InitializeAsync();

            return _shopWebAppFactory.GetHostHttpClient();
        }
        else return null;
    }

    protected override async Task<HttpClient> CreateSettingsApiWebHttpClientAsync(int settingsApiHttpPort,
        int settingsApiHttpsPort,
        TestServer signalRServer, 
        Action<AlchemyContext>? fillTestData = null)
    {
        _settingsApiFactory = SettingsApiHelper.CreateSettingsApiWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(settingsDbConnectionStringSection,
            settingsDatabase,
            settingsApiHttpPort,
            settingsApiHttpsPort,
            signalRServer, 
            fillTestData : fillTestData);

        if (_settingsApiFactory is IAsyncLifetime asyncLifetimeSettingsApiFactory)
            await asyncLifetimeSettingsApiFactory.InitializeAsync();

        return _settingsApiFactory.GetHostHttpClient();
    }
}