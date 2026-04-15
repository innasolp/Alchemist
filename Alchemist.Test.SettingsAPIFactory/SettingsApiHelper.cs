using Alchemist.Product.Data;
using Microsoft.AspNetCore.TestHost;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.SettingsAPIFactory;

internal class SettingsAPIWebAppFactoryTestDataImpl(string connectionString, TestServer signalRServer, int httpPort, int httpsPort, Action<AlchemyContext> fillTestData, bool ensureDeleted = true)
    : SettingsAPIContextWebAppFactory(connectionString, signalRServer, httpPort, httpsPort, ensureDeleted)
{
    private readonly Action<AlchemyContext> _fillTestData = fillTestData;

    protected override void FillTestData(AlchemyContext dbContext)
    {
        _fillTestData(dbContext);
    }
}

public static class SettingsApiHelper
{
    public static HttpClient CreateSettingsApiHttpClient(string alchemyDbConnectionString, int httpPort, int httpsPort, TestServer signalRTestServer, bool ensureDeleted = false)
    {
        var settingsApiFactory = new SettingsAPIContextWebAppFactory(alchemyDbConnectionString, signalRTestServer, httpPort, httpsPort, ensureDeleted);

        var settingsApiHttpClient = settingsApiFactory.Server.CreateClient();
        settingsApiHttpClient.BaseAddress = new Uri(settingsApiFactory.ServerAddress);

        return settingsApiHttpClient;
    }

    public static HttpClient CreateSettingsApiHttpClient(string alchemyDbConnectionString, int httpPort, int httpsPort, TestServer signalRTestServer, Action<AlchemyContext> fillTestData, bool ensureDeleted = false)
    {
        var settingsApiFactory = new SettingsAPIWebAppFactoryTestDataImpl(alchemyDbConnectionString, signalRTestServer, httpPort, httpsPort, fillTestData, ensureDeleted);

        var settingsApiHttpClient = settingsApiFactory.Server.CreateClient();
        settingsApiHttpClient.BaseAddress = new Uri(settingsApiFactory.ServerAddress);

        return settingsApiHttpClient;
    }

    public static HttpClient CreateSettingsApiHttpClient<TTestDbContainer, TDbRespawner, TDbChecker>
        (string alchemyDbConnectionStringSection,
        string database,
        int httpPort,
        int httpsPort,
        TestServer signalRTestServer, 
        int dbPort = 5432,
        string dbUser = "postgres",
        string dbPawword = "P@ssw0rd",
        Action<AlchemyContext>? fillTestData = null)
        where TTestDbContainer : class, ITestDbContainer, new()
        where TDbRespawner : class, IDatabaseRespawner, new()
        where TDbChecker : class, IDbChecker, new()
    {        
        var settingsApiFactory = new SettingsApiConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(alchemyDbConnectionStringSection,
            database,
            dbPort, dbUser, dbPawword,
            httpPort, httpsPort,
            signalRTestServer,
            fillTestData : fillTestData);

        return settingsApiFactory.GetHostHttpClient();
    }

    public static SettingsApiConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>
        CreateSettingsApiWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>
        (string alchemyDbConnectionStringSection,
        string database,
        int httpPort,
        int httpsPort,
        TestServer signalRTestServer, 
        int dbPort = 5432,
        string dbUser = "postgres",
        string dbPawword = "P@ssw0rd",
        Action<AlchemyContext>? fillTestData = null)
        where TTestDbContainer : class, ITestDbContainer, new()
        where TDbRespawner : class, IDatabaseRespawner, new()
        where TDbChecker : class, IDbChecker, new()
    {        
        return new SettingsApiConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(alchemyDbConnectionStringSection,
            database,
            dbPort, dbUser, dbPawword,
            httpPort, httpsPort,
            signalRTestServer,
            fillTestData : fillTestData);
    }
}