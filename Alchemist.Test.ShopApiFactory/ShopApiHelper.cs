using Alchemist.Product.Data;
using Microsoft.AspNetCore.TestHost;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.ShopApiFactory;

internal class ShopAPIWebAppFactoryTestDataImpl(string connectionString, TestServer signalRServer, int httpPort, int httpsPort, Action<AlchemyContext> fillTestData, 
    bool ensureDeleted = true)
    : ShopAPIContextWebAppFactory(connectionString, signalRServer, httpPort, httpsPort, ensureDeleted)
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
        var shopApiWebAppFactory = new ShopAPIContextWebAppFactory(alchemyDbConnectionString, signalRTestServer, httpPort, httpsPort, ensureDeleted);
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

    public static HttpClient CreateShopApiClient<TTestDbContainer, TDbRespawner, TDbChecker>(string alchemyDbConnectionStringSection,
        string database,
        int httpPort,
        int httpsPort,
        TestServer signalRTestServer,
        Action<AlchemyContext>? fillTestData = null, 
        int dbPort = 5432,
        string dbUser = "postgres",
        string dbPassword = "P@ssw0rd")
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
     where TDbChecker : class, IDbChecker, new()    
    {
        var shopApiWebAppFactory = new ShopApiConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(alchemyDbConnectionStringSection,
            database,
            dbPort,
            dbUser,
            dbPassword,
            httpPort, httpsPort, 
            signalRTestServer, fillTestData: fillTestData);
        var httpClient = shopApiWebAppFactory.GetHostHttpClient();
        return httpClient;
    }    

    public static ShopApiConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker> 
        CreateShopApiWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(string alchemyDbConnectionStringSection,
        string database,
        int httpPort,
        int httpsPort,
        TestServer signalRTestServer,
        Action<AlchemyContext>? fillTestData = null, 
        int dbPort = 5432,
        string dbUser = "postgres",
        string dbPassword = "P@ssw0rd")
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
        where TDbChecker : class, IDbChecker, new()    
        {
        var shopApiWebAppFactory = new ShopApiConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(alchemyDbConnectionStringSection,
            database,
            dbPort,
            dbUser,
            dbPassword,
            httpPort, httpsPort, 
            signalRTestServer, fillTestData: fillTestData);
        
        return shopApiWebAppFactory;
    }
}