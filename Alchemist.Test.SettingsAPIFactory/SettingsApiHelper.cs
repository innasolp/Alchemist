using Alchemist.Product.Data;
using Microsoft.AspNetCore.TestHost;

namespace Alchemist.Test.SettingsAPIFactory;

internal class SettingsAPIWebAppFactoryTestDataImpl(string connectionString, TestServer signalRServer, int httpPort, int httpsPort, Action<AlchemyContext> fillTestData, bool ensureDeleted = true)
    : SettingsAPIWebAppFactory(connectionString, signalRServer, httpPort, httpsPort, ensureDeleted)
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
        var settingsApiFactory = new SettingsAPIWebAppFactory(alchemyDbConnectionString, signalRTestServer, httpPort, httpsPort, ensureDeleted);

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
}