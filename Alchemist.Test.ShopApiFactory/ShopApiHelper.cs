using Alchemist.Product.Data;
using Microsoft.AspNetCore.TestHost;

namespace Alchemist.Test.ShopApiFactory;

internal class ShopAPIWebAppFactoryTestDataImpl(string connectionString, TestServer signalRServer, int httpPort, int httpsPort, Action<AlchemyContext> fillTestData, 
    bool ensureDeleted = true)
    : ShopAPIWebAppFactory(connectionString, signalRServer, httpPort, httpsPort, ensureDeleted)
{
    private readonly Action<AlchemyContext> _fillTestData = fillTestData;

    protected override void FillTestData(AlchemyContext dbContext)
    {
        _fillTestData(dbContext);
    }
}

public static class ShopApiHelper
{
    public static HttpClient CreateShopApiClient(string alchemyDbConnectionString, int httpPort, int httpsPort, TestServer signalRTestServer, bool ensureDeleted = true)
    {
        var shopApiWebAppFactory = new ShopAPIWebAppFactory(alchemyDbConnectionString, signalRTestServer, httpPort, httpsPort, ensureDeleted);
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(shopApiWebAppFactory.ServerAddress)
        };
        return httpClient;
    }

    public static HttpClient CreateShopApiClient(string alchemyDbConnectionString, int httpPort, int httpsPort, TestServer signalRTestServer,
        Action<AlchemyContext> fillTestData, 
        bool ensureDeleted = true)
    {
        var shopApiWebAppFactory = new ShopAPIWebAppFactoryTestDataImpl(alchemyDbConnectionString, signalRTestServer, httpPort, httpsPort, fillTestData, ensureDeleted);
        var httpClient = shopApiWebAppFactory.CreateClient();
        httpClient.BaseAddress = new Uri(shopApiWebAppFactory.ServerAddress);
        return httpClient;
    }
}
