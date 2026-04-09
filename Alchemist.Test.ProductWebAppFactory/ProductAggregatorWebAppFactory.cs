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
    TestServer signalRTestServer)
    : TestWebAppKestrelFactory<ProductWebAppProgramm>(httpPort, httpsPort), IAsyncLifetime
{
    private HttpClient? _shopWebAppApiClient;

    private HttpClient? _importSettingsWebAppApiClient;

    private TestHostServerWebAppFactory<ShopWebAppProgram>? _shopWebAppFactory;

    private TestHostServerWebAppFactory<ImportSettingsWebAppProgramm>? _settingsWebAppFactory;

    protected abstract TestHostServerWebAppFactory<ImportSettingsWebAppProgramm> CreateSettingsWebAppFactory(int settingsApiHttpPort,
        int settingsApiHttpsPort,
    int settingsWebAppApiHttpPort, 
    int settingsWebAppApiHttspPort,
    string settingsDataBaseConnectionStringSection,
     string settingsDatabase,
    TestServer signalRTestServer,
    int shopWebAppApiHttpPort, int shopWebAppApiHttspPort);

    protected abstract TestHostServerWebAppFactory<ShopWebAppProgram> CreateShopWebAppFactory(int shopApiHttpPort, int shopApiHttpsPort,
     int shopWebAppApiHttpPort, int shopWebAppApiHttspPort, TestServer signalRTestServer,
     string dataBaseConnectionStringSection, 
     string database);

    public async Task InitializeAsync()
    {
        _shopWebAppFactory = CreateShopWebAppFactory(shopApiHttpPort, shopApiHttpsPort, shopWebAppApiHttpPort, shopWebAppApiHttspPort,
            signalRTestServer, shopDataBaseConnectionStringSection, shopDatabase);

        if (_shopWebAppFactory is IAsyncLifetime asyncLifetimeShopWebAppFactory)
            await asyncLifetimeShopWebAppFactory.InitializeAsync();

        _shopWebAppApiClient = _shopWebAppFactory.GetHostHttpClient();

        _settingsWebAppFactory = CreateSettingsWebAppFactory(settingsApiHttpPort, settingsApiHttpsPort, settingsWebAppApiHttpPort, settingsWebAppApiHttspPort,
            settingsDataBaseConnectionStringSection, settingsDatabase, signalRTestServer,
            shopWebAppApiHttpPort, shopWebAppApiHttspPort);

        if (_settingsWebAppFactory is IAsyncLifetime asyncLifetimeSettingsWebAppFactory)
            await asyncLifetimeSettingsWebAppFactory.InitializeAsync();

        _importSettingsWebAppApiClient = _settingsWebAppFactory.GetHostHttpClient();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_shopWebAppFactory is IAsyncLifetime asyncLifetimeShopWebAppFactory)
            await asyncLifetimeShopWebAppFactory.DisposeAsync();

        _shopWebAppApiClient?.Dispose();

        if (_settingsWebAppFactory is IAsyncLifetime asyncLifetimeSettingsWebAppFactory)
            await asyncLifetimeSettingsWebAppFactory.InitializeAsync();

        _importSettingsWebAppApiClient?.Dispose();
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