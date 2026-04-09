using Alchemist.Test.ProductWebAppFactory;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;
using TestCommon = Alchemist.Product.WebApp.IntegrationTest.Infrastructure.Common;

namespace Alchemist.Product.WebApp.IntegrationTest.Infrastructure;

public class ProductWebAppTestContainerLifetimeFactory(int httpPort, int httpsPort,
    int shopApiHttpPort, int shopApiHttpsPort,
     int shopWebAppApiHttpPort, int shopWebAppApiHttspPort,
    int settingsApiHttpPort, int settingsApiHttpsPort,
    int settingsWebAppApiHttpPort, int settingsWebAppApiHttspPort, string database)
    : ProductWebAppLifetimeFactory(httpPort, httpsPort,
        shopApiHttpPort, shopApiHttpsPort,
        shopWebAppApiHttpPort, shopWebAppApiHttspPort,
        settingsApiHttpPort, settingsApiHttpsPort,
        settingsWebAppApiHttpPort, settingsWebAppApiHttspPort,
        TestCommon.SignalRTestServer)
{
    private readonly PostgreSqlContainer _postgreSqlContainer = PostgresqlTestContainerHelper.BuildPostgreSqlContainer(Guid.NewGuid().ToString());

    public override async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await base.InitializeAsync();
    }

    protected override async Task<string> GetSettingsDbConnectionString()
    {
        return _postgreSqlContainer.BuildConnectionString(database ?? "test_ci_db");
    }

    protected override async Task<string> GetShopDbConnectionString()
    {
        return _postgreSqlContainer.BuildConnectionString(database ?? "test_ci_db");
    }
    protected override async Task LifetimeDisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await base.LifetimeDisposeAsync();
    }
}