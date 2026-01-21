using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.Log;
using Alchemist.Test.SignalRWebAppFactory;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;

namespace Alchemist.Product.ImportSettingsWebApp.IntegrationTest.Infrastructure;

public class ImportSettingsWebAppTestContainerLifetimeFactory(bool isApi, int httpPort, int httpsPort,
    int settingsApiHttpPort, int settingsApiHttpsPort,
    string database,
    int? shopApiHttpPort = null, int? shopApiHttpsPort = null,
    int? shopWebappApiHttpPort = null, int? shopWebAppApiHttpsPort = null
    )
    : ImportSettingsWebLifetimeFactory(isApi,
        httpPort, httpsPort,
        settingsApiHttpPort, settingsApiHttpsPort,
        new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server,
        shopApiHttpPort, shopApiHttpsPort,
        shopWebappApiHttpPort, shopWebAppApiHttpsPort)
{
    private readonly PostgreSqlContainer _postgreSqlContainer = PostresqlTestContainerHelper.BuildPostgreSqlContainer(Guid.NewGuid().ToString());

    public override async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await base.InitializeAsync();
    }

    protected override async Task<string> GetShopSettingsDbConnectionString()
    {
        return _postgreSqlContainer.BuildConnectionString(database ?? "test_ci_db");
    }

    protected override async Task LifetimeDisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await base.LifetimeDisposeAsync();
    }
}