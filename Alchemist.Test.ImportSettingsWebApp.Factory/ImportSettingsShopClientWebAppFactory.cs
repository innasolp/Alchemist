using Alchemist.Product.Data;
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
    int? shopWebappApiHttpPort = null, int? shopWebAppApiHttpsPort = null,
    Action<AlchemyContext>? fillShopTestData = null,
    Action<AlchemyContext>? fillSettingsTestData = null
    ) : TestWebAppKestrelFactory<ImportSettingsWebAppProgramm>(httpPort, httpsPort), IAsyncLifetime
{
    private HttpClient? _settingsApiHttpClient;

    private HttpClient? _shopWebAppApiHttpClient = null;

    protected abstract Task<HttpClient?> CreateShopWebAppHttpClientAsync(int? shopWebappApiHttpPort,
        int? shopWebAppApiHttpsPort, 
        int? shopApiHttpPort, 
        int? shopApiHttpsPort,
        TestServer signalRServer,
        Action<AlchemyContext>? fillTestData = null);
    
    protected abstract Task<HttpClient> CreateSettingsApiWebHttpClientAsync(
        int settingsApiHttpPort, 
        int settingsApiHttpsPort, 
        TestServer signalRServer,
        Action<AlchemyContext>? fillTestData = null);

    public virtual async Task InitializeAsync()
    {
       _shopWebAppApiHttpClient = await CreateShopWebAppHttpClientAsync(shopWebappApiHttpPort,
                shopWebAppApiHttpsPort,
                    shopApiHttpPort,
                    shopApiHttpsPort,
                    signalRTestServer,
                    fillShopTestData);

        _settingsApiHttpClient = await CreateSettingsApiWebHttpClientAsync(settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer, fillSettingsTestData);
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
    { }

    Task IAsyncLifetime.DisposeAsync()
    {
        return LifetimeDisposeAsync();
    }
}