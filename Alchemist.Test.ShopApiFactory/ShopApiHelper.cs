using Alchemist.Common;
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
    public static HttpClient CreateShopApiClient(string connectionStringSection, int httpPort, int httpsPort, TestServer signalRTestServer, bool ensureDeleted = true)
    {
        var alchemyDbConnectionString = ConfigurationHelper.GetConnectionString(connectionStringSection);
        var shopApiWebAppFactory = new ShopAPIWebAppFactory(alchemyDbConnectionString, signalRTestServer, httpPort, httpsPort, ensureDeleted);
        return shopApiWebAppFactory.CreateClient();
    }

    public static HttpClient CreateShopApiClient(string connectionStringSection, int httpPort, int httpsPort, TestServer signalRTestServer,
        Action<AlchemyContext> fillTestData, 
        bool ensureDeleted = true)
    {
        var alchemyDbConnectionString = ConfigurationHelper.GetConnectionString(connectionStringSection);
        var shopApiWebAppFactory = new ShopAPIWebAppFactoryTestDataImpl(alchemyDbConnectionString, signalRTestServer, httpPort, httpsPort, fillTestData, ensureDeleted);
        return shopApiWebAppFactory.CreateClient();
    }
}
