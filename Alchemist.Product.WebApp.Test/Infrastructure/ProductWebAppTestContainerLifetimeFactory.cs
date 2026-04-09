using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.ProductWebAppFactory;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;

namespace Alchemist.Product.WebApp.Test.Infrastructure;

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
        Common.SignalRTestServer)
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

    protected override async Task<HttpClient> CreateSettingsWebAppApiHttpClient()
    {
        var connectionString = await GetSettingsDbConnectionString();
        return ImportSettingsWebAppHelper.CreateImportSettingsWebAppApiHttpClient(connectionString,
                  settingsWebAppApiHttpPort, settingsWebAppApiHttspPort,
            default,
            settingsApiHttpPort,
          settingsApiHttpsPort, Common.SignalRTestServer,
          (dbContext) => Common.FillTestData(dbContext, [1, 2, 3, 4]));
    }

    protected override async Task LifetimeDisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await base.LifetimeDisposeAsync();
    }
}