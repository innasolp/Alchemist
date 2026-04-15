using Alchemist.Product.Data;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Alchemist.Test.ProductWebAppFactory;

public abstract class ProductAggregatorWebAppFactory(int httpPort, int httpsPort,
    int shopApiHttpPort, int shopApiHttpsPort,
     int shopWebAppApiHttpPort, int shopWebAppApiHttspPort,
     string shopDataBaseConnectionStringSection,
     string shopDatabase,
    int settingsApiHttpPort, int settingsApiHttpsPort,
    int settingsWebAppApiHttpPort, int settingsWebAppApiHttspPort,
    string settingsDataBaseConnectionStringSection,
     string settingsDatabase,
    TestServer signalRTestServer,
    Action<AlchemyContext>? fillShopTestData = null,
    Action<AlchemyContext>? fillSettingsTestData = null)
    : TestWebAppKestrelFactory<ProductWebAppProgramm>(httpPort, httpsPort), IAsyncLifetime
{
    private HttpClient? _shopWebAppApiClient;

    private HttpClient? _importSettingsWebAppApiClient;

    protected abstract Task<HttpClient> CreateSettingsWebAppHttpClientAsync(int settingsApiHttpPort,
        int settingsApiHttpsPort,
    int settingsWebAppApiHttpPort, 
    int settingsWebAppApiHttspPort,
    string settingsDataBaseConnectionStringSection,
     string settingsDatabase,
    TestServer signalRTestServer,
    HttpClient shopWebAppHttpClient,
    Action<AlchemyContext>? fillTestData);

    protected abstract Task<HttpClient> CreateShopWebAppHttpClientAsync(int shopApiHttpPort, int shopApiHttpsPort,
     int shopWebAppApiHttpPort, int shopWebAppApiHttspPort, TestServer signalRTestServer,
     string dataBaseConnectionStringSection, 
     string database,
     Action<AlchemyContext>? fillTestData);

    public async Task InitializeAsync()
    {
        _shopWebAppApiClient = await CreateShopWebAppHttpClientAsync(shopApiHttpPort, shopApiHttpsPort, shopWebAppApiHttpPort, shopWebAppApiHttspPort,
            signalRTestServer, shopDataBaseConnectionStringSection, shopDatabase, fillShopTestData);

        _importSettingsWebAppApiClient = await CreateSettingsWebAppHttpClientAsync(settingsApiHttpPort, settingsApiHttpsPort, settingsWebAppApiHttpPort, settingsWebAppApiHttspPort,
            settingsDataBaseConnectionStringSection, settingsDatabase, signalRTestServer,
            _shopWebAppApiClient, fillSettingsTestData);
    }

    protected virtual async Task DisposeLifetimeAsync()
    {}

    Task IAsyncLifetime.DisposeAsync()
    {
        return DisposeLifetimeAsync();
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        if (!string.IsNullOrEmpty(_shopWebAppApiClient?.BaseAddress?.AbsoluteUri))
            SetProxyHost(context.Configuration, "shopCluster", "user", _shopWebAppApiClient.BaseAddress.AbsoluteUri);

        if (!string.IsNullOrEmpty(_importSettingsWebAppApiClient?.BaseAddress?.AbsoluteUri))
            SetProxyHost(context.Configuration, "importSettingsCluster", "user", _importSettingsWebAppApiClient.BaseAddress.AbsoluteUri);
    }

    private static void SetProxyHost(IConfiguration configuration, string cluster, string destinationName, string destinationHost)
    {
        var hostSectionPath = $"ReverseProxy:Clusters:{cluster}:Destinations:{destinationName}:Address";
        var section = configuration.GetSection(hostSectionPath);
        section.Value = destinationHost;
    }
}