using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.ShopWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Alchemist.Test.ProductWebAppFactory;

public abstract class ProductWebAppLifetimeFactory(int httpPort, int httpsPort,
    int shopApiHttpPort, int shopApiHttpsPort,
     int shopWebAppApiHttpPort, int shopWebAppApiHttspPort,
    int settingsApiHttpPort, int settingsApiHttpsPort,
    int settingsWebAppApiHttpPort, int settingsWebAppApiHttspPort,
    TestServer signalRTestServer)
    : TestWebAppKestrelFactory<ProductWebAppProgramm>(httpPort, httpsPort), IAsyncLifetime
{
    private HttpClient? _shopWebAppApiClient;

    private HttpClient? _importSettingsWebAppApiClient;

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        if (!string.IsNullOrEmpty(_shopWebAppApiClient.BaseAddress.AbsoluteUri))
            SetProxyHost(context.Configuration, "shopCluster", "user", _shopWebAppApiClient.BaseAddress.AbsoluteUri);

        if (!string.IsNullOrEmpty(_importSettingsWebAppApiClient.BaseAddress.AbsoluteUri))
            SetProxyHost(context.Configuration, "importSettingsCluster", "user", _importSettingsWebAppApiClient.BaseAddress.AbsoluteUri);
    }

    private static void SetProxyHost(IConfiguration configuration, string cluster, string destinationName, string destinationHost)
    {
        var hostSectionPath = $"ReverseProxy:Clusters:{cluster}:Destinations:{destinationName}:Address";
        var section = configuration.GetSection(hostSectionPath);
        section.Value = destinationHost;
    }

    protected abstract Task<string> GetShopDbConnectionString();
    protected abstract Task<string> GetSettingsDbConnectionString();

    protected virtual async Task<HttpClient> CreateShopWebAppApiHttpClient()
    {
        var connectionString = await GetShopDbConnectionString();
        return ShopWebAppHelper.CreateShopWebAppApiHttpClient(connectionString, 
            shopWebAppApiHttpPort, shopWebAppApiHttspPort,
            shopApiHttpPort, shopApiHttpsPort,
            signalRTestServer);
    }

    protected virtual async Task<HttpClient> CreateSettingsWebAppApiHttpClient()
    {
        var connectionString = await GetSettingsDbConnectionString();
        return ImportSettingsWebAppHelper.CreateImportSettingsWebAppApiHttpClient(connectionString,
                  settingsWebAppApiHttpPort, settingsWebAppApiHttspPort,
            default,
            settingsApiHttpPort,
          settingsApiHttpsPort, signalRTestServer);
    }

    public virtual async Task InitializeAsync()
    {
        _shopWebAppApiClient = await CreateShopWebAppApiHttpClient();   
        _importSettingsWebAppApiClient = await CreateSettingsWebAppApiHttpClient();
    }

    protected virtual async Task LifetimeDisposeAsync()
    {
        _shopWebAppApiClient?.Dispose();
        _importSettingsWebAppApiClient?.Dispose();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return LifetimeDisposeAsync();
    }
}