using Alchemist.Product.Data;
using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.ShopWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Test.DbContainer.Abstractions;
using Test.PostresqlTestContainer;
using Xunit;

namespace Alchemist.Test.ProductWebAppFactory;

public class ProductAggregatorConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(int httpPort, int httpsPort,
    int shopApiHttpPort, int shopApiHttpsPort,
    int shopWebAppApiHttpPort, int shopWebAppApiHttpsPort,
    string shopDataBaseConnectionStringSection, string shopDatabase, 
    int settingsApiHttpPort, int settingsApiHttpsPort, 
    int settingsWebAppApiHttpPort, int settingsWebAppApiHttspPort, 
    string settingsDataBaseConnectionStringSection, string settingsDatabase, 
    TestServer signalRTestServer,
    Action<AlchemyContext>? fillShopTestData = null,
    Action<AlchemyContext>? fillSettingsTestData = null) 
    : ProductAggregatorWebAppFactory(httpPort, 
        httpsPort, 
        shopApiHttpPort, 
        shopApiHttpsPort, 
        shopWebAppApiHttpPort, 
        shopWebAppApiHttpsPort, 
        shopDataBaseConnectionStringSection, 
        shopDatabase, 
        settingsApiHttpPort, 
        settingsApiHttpsPort, 
        settingsWebAppApiHttpPort, 
        settingsWebAppApiHttspPort, 
        settingsDataBaseConnectionStringSection, 
        settingsDatabase,
        signalRTestServer, 
        fillShopTestData,
        fillSettingsTestData)
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
    where TDbChecker : class, IDbChecker, new()
{
    private ShopApiClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>? _shopWebAppFactory;

    private ImportSettingsShopHttpClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>? _settingsWebAppFactory;

    protected override async Task<HttpClient> CreateSettingsWebAppHttpClientAsync(int settingsApiHttpPort, 
        int settingsApiHttpsPort, 
        int settingsWebAppApiHttpPort,
        int settingsWebAppApiHttspPort,
        string settingsDataBaseConnectionStringSection, 
        string settingsDatabase, 
        TestServer signalRTestServer,
        HttpClient shopWebAppHttpClient, Action<AlchemyContext>? fillTestData = null)
    {
        _settingsWebAppFactory = new ImportSettingsShopHttpClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(true,
            settingsWebAppApiHttpPort,
            settingsWebAppApiHttspPort,
            shopWebAppHttpClient,
            signalRTestServer,
            settingsApiHttpPort,
            settingsApiHttpsPort,
            settingsDataBaseConnectionStringSection,
            settingsDatabase, 
            fillTestData);

        if (_settingsWebAppFactory is IAsyncLifetime asyncLifetimeSettingsWebAppFactory)
            await asyncLifetimeSettingsWebAppFactory.InitializeAsync();

        return _settingsWebAppFactory.GetHostHttpClient();
    }

    protected override async Task<HttpClient> CreateShopWebAppHttpClientAsync(int shopApiHttpPort,
        int shopApiHttpsPort, 
        int shopWebAppApiHttpPort, 
        int shopWebAppApiHttspPort, 
        TestServer signalRTestServer,
        string dataBaseConnectionStringSection,
        string database, Action<AlchemyContext>? fillTestData = null)
    {
        _shopWebAppFactory = ShopWebAppHelper.CreateShopApiClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(dataBaseConnectionStringSection,
            database,
            shopWebAppApiHttpPort,
            shopWebAppApiHttspPort,
            shopApiHttpPort,
            shopApiHttpsPort, signalRTestServer,
            fillTestData);

        if (_shopWebAppFactory is IAsyncLifetime asyncLifetimeShopWebAppFactory)
            await asyncLifetimeShopWebAppFactory.InitializeAsync();

        return _shopWebAppFactory.GetHostHttpClient();
    }

    protected override async Task DisposeLifetimeAsync()
    {
        await base.DisposeLifetimeAsync();

        if (_shopWebAppFactory is IAsyncLifetime asyncLifetimeShopWebAppFactory)
            await asyncLifetimeShopWebAppFactory.DisposeAsync();

        if (_settingsWebAppFactory is IAsyncLifetime asyncLifetimeSettingsWebAppFactory)
            await asyncLifetimeSettingsWebAppFactory.DisposeAsync();        
    }

    public async Task ResetDataAsync()
    {
        if (_shopWebAppFactory != null)
            await _shopWebAppFactory.ResetDatabaseAsync();

        if(_settingsWebAppFactory != null)
            await _settingsWebAppFactory.ResetDatabaseIfAvailableAsync();
    }
}