using Alchemist.Test.Log;
using Alchemist.Test.ShopApiFactory;
using Alchemist.Test.ShopWebAppFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;


namespace Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;

internal static class Common
{
    private static TestServer? _signalRTestServer;

    public static TestServer SignalRTestServer
    {
        get
        {
            _signalRTestServer ??= new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server;

            return _signalRTestServer;
        }
    }    

    internal static HttpClient CreateShopApiClient(string connectionStringSection, int httpPort, int httpsPort, bool ensureDeleted = true)
    {
        var alchemyDbConnectionString = GetConnectionString(connectionStringSection);
        var shopApiWebAppFactory = new ShopAPIWebAppFactory(alchemyDbConnectionString, SignalRTestServer, httpPort, httpsPort, ensureDeleted);
        return shopApiWebAppFactory.CreateClient();
    }

    private static string? GetConnectionString(string connectionStringSection)
    {
        var settings = new ConfigurationBuilder()
     .AddJsonFile("appsettings.json")
     .Build();

        var alchemyDbConnectionString = settings.GetConnectionString(connectionStringSection);
        return alchemyDbConnectionString;
    }    

    internal static HttpClient CreateSettingsApiHttpClient(string connectionStringSection, int httpPort, int httpsPort, bool ensureDeleted = false)
    {
        var alchemyDbConnectionString = GetConnectionString(connectionStringSection);
        var settingsApiFactory = new TestSettingsApiFactory(alchemyDbConnectionString, SignalRTestServer, httpPort, httpsPort, [1, 2, 3, 4], ensureDeleted);

        var settingsApiHttpClient = settingsApiFactory.Server.CreateClient();
        settingsApiHttpClient.BaseAddress = new Uri(settingsApiFactory.ServerAddress);

        return settingsApiHttpClient;
    }    

    internal static ShopWebAppFactory CreateShopWebAppApiFactory(string connectionStringSection, int httpPort, int httpsPort, int shopApiHttpPort, int shopApiHttpsPort)
    {
        var shopApiClient = CreateShopApiClient(connectionStringSection, shopApiHttpPort, shopApiHttpsPort, true);
        var shopWebAppApiFactory = new ShopWebAppFactory(true, httpPort, httpsPort, shopApiClient);
        return shopWebAppApiFactory;
    }    
}
