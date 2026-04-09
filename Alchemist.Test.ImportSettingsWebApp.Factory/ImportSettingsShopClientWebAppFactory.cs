using Alchemist.Settings.RestAPIClient;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopSettings.Interfaces;
using Xunit;

namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public abstract class ImportSettingsShopClientWebAppFactory(bool isApi, int httpPort, int httpsPort,
    int settingsApiHttpPort, int settingsApiHttpsPort,
    TestServer signalRTestServer,
    int? shopApiHttpPort = null, int? shopApiHttpsPort = null,
    int? shopWebappApiHttpPort = null, int? shopWebAppApiHttpsPort = null
    ) : TestWebAppKestrelFactory<ImportSettingsWebAppProgramm>(httpPort, httpsPort), IAsyncLifetime
{
    private HttpClient? _settingsApiHttpClient;

    private HttpClient? _shopWebAppApiHttpClient = null;

    private TestHostServerWebAppFactory<SettingsAPIProgram>? _settingsApiFactory;

    private TestHostServerWebAppFactory<ShopWebAppProgram>? _shopWebAppFactory;

    //protected abstract Task<string> GetShopSettingsDbConnectionString();

    //protected abstract Task<HttpClient> CreateShopWebAppHttpClient(int shopWebappApiHttpPort,
    //    int shopWebAppApiHttpsPort, 
    //    int shopApiHttpPort, 
    //    int shopApiHttpsPort,
    //    TestServer signalRServer);

    protected abstract TestHostServerWebAppFactory<ShopWebAppProgram> CreateShopWebAppFactory(int shopWebappApiHttpPort,
        int shopWebAppApiHttpsPort, 
        int shopApiHttpPort, 
        int shopApiHttpsPort,
        TestServer signalRServer);

    //protected abstract Task<HttpClient> CreateSettingsApiHttpClient(int settingsApiHttpPort, int settingsApiHttpsPort, TestServer signalRServer);
    
    protected abstract TestHostServerWebAppFactory<SettingsAPIProgram> CreateSettingsApiWebAppFactory(int settingsApiHttpPort, 
        int settingsApiHttpsPort, 
        TestServer signalRServer);

    public virtual async Task InitializeAsync()
    {
        var shopPorts = new int?[] { shopWebappApiHttpPort, shopWebAppApiHttpsPort, shopApiHttpPort, shopApiHttpsPort };
        if (shopPorts.All(p => p.HasValue))
        {
            _shopWebAppFactory = CreateShopWebAppFactory(shopWebappApiHttpPort!.Value,
                shopWebAppApiHttpsPort!.Value,
                    shopApiHttpPort!.Value,
                    shopApiHttpsPort!.Value,
                    signalRTestServer);

            if (_shopWebAppFactory is IAsyncLifetime asyncLifetimeShopFactory)
                await asyncLifetimeShopFactory.InitializeAsync();

            _shopWebAppApiHttpClient = _shopWebAppFactory.GetHostHttpClient();
        }

        _settingsApiFactory = CreateSettingsApiWebAppFactory(settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer);

        if (_settingsApiFactory is IAsyncLifetime asyncLifetimeSettingsApiFactory)
            await asyncLifetimeSettingsApiFactory.InitializeAsync();

        _settingsApiHttpClient = _settingsApiFactory.GetHostHttpClient();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (isApi)
            builder.UseSetting("api", "true");

        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.InterceptImplementation<IShopSettingsDataService, SettingsAPIClient>(new SettingsAPIClient(_settingsApiHttpClient));

        if (!isApi && !string.IsNullOrEmpty(_shopWebAppApiHttpClient?.BaseAddress?.AbsoluteUri))
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
        if(_shopWebAppFactory is IAsyncLifetime asyncLifetimeShopFactory)
            await asyncLifetimeShopFactory.DisposeAsync();

        _shopWebAppApiHttpClient?.Dispose();

        if (_settingsApiFactory is IAsyncLifetime asyncLifetimeSettingsFactory)
            await asyncLifetimeSettingsFactory.DisposeAsync();

        _settingsApiHttpClient?.Dispose();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return LifetimeDisposeAsync();
    }
}
