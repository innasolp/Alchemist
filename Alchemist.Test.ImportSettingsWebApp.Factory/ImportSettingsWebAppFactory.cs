using Alchemist.DataService.Interfaces;
using Alchemist.Settings.RestAPIClient;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public class ImportSettingsWebAppFactory(bool isApi, string? shopApiHost, int httpPort, int httpsPort,
    HttpClient settingsApiClient) : TestWebAppKestrelFactory<ImportSettingsWebAppProgramm>(httpPort, httpsPort)
{
    private readonly bool _isApi = isApi;

    private readonly string? _shopApiHost = shopApiHost;

    private readonly HttpClient _settingsApiClient = settingsApiClient;

    public ImportSettingsWebAppFactory(bool isApi, int httpPort, int httpsPort,HttpClient settingsApiClient) 
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

        if(!_isApi && !string.IsNullOrEmpty(_shopApiHost)) SetProxyHost(context.Configuration, "shopCluster", "user", _shopApiHost);
    }

    private static void SetProxyHost(IConfiguration configuration, string cluster, string destinationName, string destinationHost)
    {
        var hostSectionPath = $"ReverseProxy:Clusters:{cluster}:Destinations:{destinationName}:Address";
        var section = configuration.GetSection(hostSectionPath);
        section.Value = destinationHost;
    }
}
