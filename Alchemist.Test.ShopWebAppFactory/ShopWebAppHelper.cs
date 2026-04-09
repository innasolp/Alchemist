using Alchemist.Product.Data;
using Alchemist.Test.ShopApiFactory;
using Microsoft.AspNetCore.TestHost;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.ShopWebAppFactory;

public static class ShopWebAppHelper
{
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

    public static HttpClient CreateShopWebAppApiHttpClient<TTestDbContainer, TDbRespawner>(string connectionStringSection,
        string shopDatabase,
        int httpPort,
        int httpsPort,
        int shopApiHttpPort,
        int shopApiHttpsPort,
        TestServer signalRTestServer,
        Action<AlchemyContext> fillTestData)
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
    {
        var shopApiClient = ShopApiHelper.CreateShopApiClient<TTestDbContainer, TDbRespawner>
            (connectionStringSection,
            shopDatabase,
            shopApiHttpPort,
            shopApiHttpsPort,
            signalRTestServer,
            fillTestData);
        var shopWebAppApiFactory = new ShopWebAppFactory(true, httpPort, httpsPort, shopApiClient);
        var httpClient = shopWebAppApiFactory.GetHostHttpClient();
        return httpClient;
    }

}