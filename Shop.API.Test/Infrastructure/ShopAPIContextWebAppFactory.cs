using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.DBApiWebAppFactory.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;

namespace Shop.API.Test.Infrastructure;

public abstract class ShopAPIContextWebAppFactory : DbContextWebAppFactory<ShopAPIProgram, AlchemyContext>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    private readonly string _host = Guid.NewGuid().ToString();

    protected ShopAPIContextWebAppFactory()
    {
        _postgreSqlContainer = PostgresqlTestContainerHelper.BuildPostgreSqlContainer(_host);
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Shops.Add(new Alchemist.Product.Data.Shop { Name = "TestShop", Url = "https://testshop1" });
        dbContext.SaveChanges();
    }

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddAlchemyPostgresContextFactory(optionsBuilder =>
        optionsBuilder.UseNpgsql(_postgreSqlContainer.BuildConnectionString(DataBase)));
    }
    public Task InitializeAsync()
    {
        return _postgreSqlContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}