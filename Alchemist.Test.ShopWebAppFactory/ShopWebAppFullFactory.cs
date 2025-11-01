using Alchemist.Test.Log;
using Alchemist.Test.ShopApiFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Alchemist.Test.ShopWebAppFactory;

public class ShopWebAppFullFactory(bool isApi, string connectionSection, int httpPort, int httpsPort,
    int shopAPIHttpPort, int shopAPIHttpsPort, TestServer signalRTestServer) 
    : ShopWebAppFactory(isApi, httpPort, httpsPort, GetShopApiHttpClient(connectionSection, shopAPIHttpPort, shopAPIHttpsPort, signalRTestServer)) 
{
    private static HttpClient GetShopApiHttpClient(string connectionSection, int shopAPIHttpPort, int shopAPIHttpsPort, TestServer signalRTestServer)
    {
        var settings = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

        var alchemyDbConnectionString = settings.GetConnectionString(connectionSection);
        var shopAPIWebAppFactory = new ShopAPIWebAppFactory(alchemyDbConnectionString, signalRTestServer, shopAPIHttpPort, shopAPIHttpsPort);
        return shopAPIWebAppFactory.CreateClient();
    }

    public ShopWebAppFullFactory(bool isApi, string connectionSection, int httpPort, int httpsPort,
        int shopAPIHttpPort, int shopAPIHttpsPort) : 
        this(isApi, connectionSection, httpPort, httpsPort, shopAPIHttpPort, shopAPIHttpsPort, 
            new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server)
    {
    }    
}
