using Alchemist.Test.Log;
using Alchemist.Test.ShopWebAppFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;

namespace Alchemist.Product.ShopWebApp.IntegratonTest.Infrastructure;

public class ShopWebAppLifetimeTestContainerFactory(bool isApi, int httpPort, int httpsPort, int shopAPIHttpPort, int shopAPIHttpsPort, string database)
    : ShopWebAppLifetimeFactory(isApi, httpPort, httpsPort, shopAPIHttpPort, shopAPIHttpsPort, new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server)
{
    private readonly string _database = database;
    private readonly PostgreSqlContainer _postgreSqlContainer = PostgresqlTestContainerHelper.BuildPostgreSqlContainer(Guid.NewGuid().ToString());

    public override async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await base.InitializeAsync();
    }

    protected override async Task<string> GetShopDbConnectionString()
    {
        return _postgreSqlContainer.BuildConnectionString(_database ?? "test_ci_db");
    }

    protected override async Task LifetimeDisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await base.LifetimeDisposeAsync();
    }
}