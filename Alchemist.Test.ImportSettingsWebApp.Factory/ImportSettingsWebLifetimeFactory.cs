using Alchemist.Settings.RestAPIClient;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.ShopWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopSettings.Interfaces;
using Xunit;

namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public abstract class ImportSettingsWebLifetimeFactory(bool isApi, int httpPort, int httpsPort,
    int settingsApiHttpPort, int settingsApiHttpsPort,
    TestServer signalRTestServer,
    int? shopApiHttpPort = null, int? shopApiHttpsPort = null,
    int? shopWebappApiHttpPort = null, int? shopWebAppApiHttpsPort = null
    )
    : TestWebAppKestrelFactory<ImportSettingsWebAppProgramm>(httpPort, httpsPort), IAsyncLifetime
{
    private readonly bool _isApi = isApi;

    private HttpClient? _settingsApiHttpClient;

    private HttpClient? _shopWebAppApiHttpClient = null;

    protected abstract Task<string> GetShopSettingsDbConnectionString();

    protected virtual async Task<HttpClient> CreateSettingsApiHttpClient()
    {
        var connectionString = await GetShopSettingsDbConnectionString();

        InitializeShopWebAppHttpClientIfAvailable(connectionString);

        return SettingsApiHelper.CreateSettingsApiHttpClient(connectionString, settingsApiHttpPort, 
            settingsApiHttpsPort, signalRTestServer);
    }

    protected virtual void InitializeShopWebAppHttpClientIfAvailable(string connectionString)
    {
        var shopPorts = new int?[] { shopWebappApiHttpPort, shopWebAppApiHttpsPort, shopApiHttpPort, shopApiHttpsPort };
        if (shopPorts.All(p => p.HasValue))
            _shopWebAppApiHttpClient = ShopWebAppHelper.CreateShopWebAppApiHttpClient(connectionString,
                shopWebappApiHttpPort.Value, shopWebAppApiHttpsPort.Value,
                shopApiHttpPort.Value, shopApiHttpsPort.Value,
                signalRTestServer);
    }

    public virtual async Task InitializeAsync()
    {
        _settingsApiHttpClient = await CreateSettingsApiHttpClient();       
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (_isApi)
            builder.UseSetting("api", "true");

        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.InterceptImplementation<IShopSettingsDataService, SettingsAPIClient>(new SettingsAPIClient(_settingsApiHttpClient));

        if (!_isApi && !string.IsNullOrEmpty(_shopWebAppApiHttpClient?.BaseAddress?.AbsoluteUri)) 
            SetProxyHost(context.Configuration, "shopCluster", "user", _shopWebAppApiHttpClient.BaseAddress.AbsoluteUri);
    }

    private static void SetProxyHost(IConfiguration configuration, string cluster, string destinationName, string destinationHost)
    {
        var hostSectionPath = $"ReverseProxy:Clusters:{cluster}:Destinations:{destinationName}:Address";
        var section = configuration.GetSection(hostSectionPath);
        section.Value = destinationHost;
    }

    protected virtual async Task LifetimeDisposeAsync()
    {
        _settingsApiHttpClient?.Dispose();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return LifetimeDisposeAsync();
    }
}