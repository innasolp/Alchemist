using Alchemist.Settings.RestAPIClient;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopSettings.Interfaces;

namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public class ImportSettingsWebAppFactory(bool isApi, string? shopWebAppApiHost, int httpPort, int httpsPort,
    HttpClient settingsApiClient) : TestWebAppKestrelFactory<ImportSettingsWebAppProgramm>(httpPort, httpsPort)
{
    private readonly bool _isApi = isApi;

    private readonly string? _shopWebAppApiHost = shopWebAppApiHost;

    private readonly HttpClient _settingsApiClient = settingsApiClient;

    public ImportSettingsWebAppFactory(bool isApi, int httpPort, int httpsPort, HttpClient settingsApiClient) 
        : this(isApi, null, httpPort, httpsPort, settingsApiClient)
    { }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (_isApi)
            builder.UseSetting("api", "true");

        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.InterceptImplementation<IShopSettingsDataService, SettingsAPIClient>(new SettingsAPIClient(_settingsApiClient));

        if(!_isApi && !string.IsNullOrEmpty(_shopWebAppApiHost)) SetProxyHost(context.Configuration, "shopCluster", "user", _shopWebAppApiHost);
    }

    private static void SetProxyHost(IConfiguration configuration, string cluster, string destinationName, string destinationHost)
    {
        var hostSectionPath = $"ReverseProxy:Clusters:{cluster}:Destinations:{destinationName}:Address";
        var section = configuration.GetSection(hostSectionPath);
        section.Value = destinationHost;
    }
}
