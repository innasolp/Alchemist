using Alchemist.Product.Data;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SettingsAPIFactory;
using Microsoft.AspNetCore.TestHost;
using Test.DbContainer.Abstractions;
using Xunit;

namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public class ImportSettingsShopHttpClientConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(bool isApi,
    int httpPort,
    int httpsPort,
    HttpClient shopWebAppClient,
    TestServer signalRTestServer,
    int settingsApiHttpPort,
    int settingsApiHttpsPort,
    string settingsDbConnectionStringSection = "ConnectionStrings:DbContext",
    string settingsDatabase = "alchemy",
    Action<AlchemyContext>? fillSettingsTestData = null
    )
    : ImportSettingsShopClientWebAppFactory(isApi, httpPort, httpsPort, settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer,
        fillSettingsTestData : fillSettingsTestData)
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
    where TDbChecker : class, IDbChecker, new()
{
    private SettingsApiConfigurationWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>? _settingsApiFactory;

    protected override async Task<HttpClient> CreateSettingsApiWebHttpClientAsync(int settingsApiHttpPort, int settingsApiHttpsPort, TestServer signalRServer, Action<AlchemyContext>? fillTestData = null)
    {
        _settingsApiFactory = SettingsApiHelper.CreateSettingsApiWebAppFactory<TTestDbContainer, TDbRespawner, TDbChecker>(settingsDbConnectionStringSection,
            settingsDatabase,
            settingsApiHttpPort,
            settingsApiHttpsPort,
            signalRServer, fillTestData : fillTestData);

        if (_settingsApiFactory is IAsyncLifetime asyncLifetimeSettingsApiFactory)
            await asyncLifetimeSettingsApiFactory.InitializeAsync();

        return _settingsApiFactory.GetHostHttpClient();
    }

    protected override async Task<HttpClient?> CreateShopWebAppHttpClientAsync(int? shopWebappApiHttpPort,
        int? shopWebAppApiHttpsPort,
        int? shopApiHttpPort,
        int? shopApiHttpsPort,
        TestServer signalRServer,
        Action<AlchemyContext>? fillTestData = null)
    {
        return shopWebAppClient;
    }

    protected override async Task LifetimeDisposeAsync()
    {
        if (_settingsApiFactory is IAsyncLifetime asyncLifetimeSettingsFactory)
            await asyncLifetimeSettingsFactory.DisposeAsync();

        await base.LifetimeDisposeAsync();
    }

    public Task ResetDatabaseIfAvailableAsync()
    {
        return _settingsApiFactory != null ? _settingsApiFactory.ResetDatabaseIfAvailableAsync() : Task.CompletedTask;
    }
}