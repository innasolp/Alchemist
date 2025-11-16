using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.ShopWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.ProductWebAppFactory;

public class ProductWebAppFactory(int httpPort, int httpsPort, HttpClient shopWebAppPiClient, HttpClient importSettingsWebAppApiClient) 
    : TestWebAppKestrelFactory<ProductWebAppProgramm>(httpPort, httpsPort)
{
    private readonly HttpClient _shopWebAppApiClient = shopWebAppPiClient;

    private readonly HttpClient _importSettingsWebAppApiClient = importSettingsWebAppApiClient;

    public ProductWebAppFactory(int httpPort, int httpsPort, string connectionString, TestServer signalRTestServer,
        int shopApiHttpPort, int shopApiHttpsPort, int shopWebAppApiHttpPort, int shopWebAppApiHttspPort,
        int settingsApiHttpPort, int settingsApiHttpsPort, int settingsWebAppApiHttpPort, int settingsWebAppApiHttspPort) :
         this(httpPort, httpsPort, 
             ShopWebAppHelper.CreateShopWebAppApiHttpClient(connectionString,
                 shopWebAppApiHttpPort, shopWebAppApiHttspPort, shopApiHttpPort,
                 shopApiHttpsPort, signalRTestServer),
             ImportSettingsWebAppHelper.CreateImportSettingsWebAppApiHttpClient(connectionString,
                  settingsWebAppApiHttpPort, settingsWebAppApiHttspPort,
            null,
            settingsApiHttpPort,
          settingsApiHttpsPort, signalRTestServer))
    {
    }

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
}