using Alchemist.Test.Log;
using Alchemist.Test.ShopApiFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;

namespace Alchemist.Test.ShopWebAppFactory;

public class ShopWebAppFullFactory(bool isApi, string connectionString, int httpPort, int httpsPort,
    int shopAPIHttpPort, int shopAPIHttpsPort, TestServer signalRTestServer) 
    : ShopWebAppFactory(isApi, httpPort, httpsPort, 
        ShopApiHelper.CreateShopApiClient(connectionString, shopAPIHttpPort, shopAPIHttpsPort, signalRTestServer)) 
{
    public ShopWebAppFullFactory(bool isApi, string connectionString, int httpPort, int httpsPort,
        int shopAPIHttpPort, int shopAPIHttpsPort) : 
        this(isApi, connectionString, httpPort, httpsPort, shopAPIHttpPort, shopAPIHttpsPort, 
            new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server)
    {
    }    
}
