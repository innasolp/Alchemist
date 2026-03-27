using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.DBApiWebAppFactory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;

namespace Shop.API.Test.Infrastructure;

public abstract class ShopAPIWebAppFactory : DbContextWebAppFactory<ShopAPIProgram, AlchemyContext>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    private readonly string _host = Guid.NewGuid().ToString();

    protected ShopAPIWebAppFactory()
    {
        _postgreSqlContainer = PostresqlTestContainerHelper.BuildPostgreSqlContainer(_host);
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Shops.Add(new Alchemist.Product.Data.Shop { Name = "TestShop", Url = "https://testshop1" });
        dbContext.SaveChanges();
    }

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContextPostgres>(optionsBuilder =>
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
