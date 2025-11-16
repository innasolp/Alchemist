using Alchemist.Product.Data;
using Alchemist.Test.ShopApiFactory;
using Microsoft.AspNetCore.TestHost;

namespace Alchemist.Test.ShopWebAppFactory;

public static class ShopWebAppHelper
{
    public static ShopWebAppFactory CreateShopWebAppApiFactory(string connectionString, 
        int httpPort, 
        int httpsPort, 
        int shopApiHttpPort, 
        int shopApiHttpsPort,
        TestServer signalRTestServer)
    {
        var shopApiClient = ShopApiHelper.CreateShopApiClient(connectionString, shopApiHttpPort, shopApiHttpsPort, signalRTestServer, true);
        var shopWebAppApiFactory = new ShopWebAppFactory(true, httpPort, httpsPort, shopApiClient);
        return shopWebAppApiFactory;
    }

    public static HttpClient CreateShopWebAppApiHttpClient(string connectionString, 
        int httpPort, 
        int httpsPort, 
        int shopApiHttpPort, 
        int shopApiHttpsPort,
        TestServer signalRTestServer)
    {
        var shopApiClient = ShopApiHelper.CreateShopApiClient(connectionString, shopApiHttpPort, shopApiHttpsPort, signalRTestServer, true);
        var shopWebAppApiFactory = new ShopWebAppFactory(true, httpPort, httpsPort, shopApiClient);
        var httpClient = shopWebAppApiFactory.CreateClient();
        httpClient.BaseAddress = new Uri(shopWebAppApiFactory.ServerAddress);
        return httpClient;
    }

    public static ShopWebAppFactory CreateShopWebAppApiFactory(string connectionString, 
        int httpPort, 
        int httpsPort, 
        int shopApiHttpPort, 
        int shopApiHttpsPort,
        TestServer signalRTestServer,
        Action<AlchemyContext> fillTestData)
    {
        var shopApiClient = ShopApiHelper.CreateShopApiClient(connectionString, shopApiHttpPort, shopApiHttpsPort, signalRTestServer, fillTestData, true);
        var shopWebAppApiFactory = new ShopWebAppFactory(true, httpPort, httpsPort, shopApiClient);
        return shopWebAppApiFactory;
    }

    public static HttpClient CreateShopWebAppApiHttpClient(string connectionString, 
        int httpPort, 
        int httpsPort, 
        int shopApiHttpPort, 
        int shopApiHttpsPort,
        TestServer signalRTestServer,
        Action<AlchemyContext> fillTestData)
    {
        var shopApiClient = ShopApiHelper.CreateShopApiClient(connectionString, shopApiHttpPort, shopApiHttpsPort, signalRTestServer, fillTestData, true);
        var shopWebAppApiFactory = new ShopWebAppFactory(true, httpPort, httpsPort, shopApiClient);
        var httpClient = shopWebAppApiFactory.CreateClient();
        httpClient.BaseAddress = new Uri(shopWebAppApiFactory.ServerAddress);
        return httpClient;
    }
}
