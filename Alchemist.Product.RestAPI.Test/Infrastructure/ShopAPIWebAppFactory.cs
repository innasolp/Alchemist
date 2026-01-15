using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.DBApiWebAppFactory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.RestAPI.Test.Infrastructure;

public abstract class ShopAPIWebAppFactory : DbContextWebAppFactory<ShopAPIProgram, AlchemyContext>
{
    public string DataBase { get; set; } = "test_ci_db";

    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Shops.Add(new Data.Shop { Name = "TestShop", Url = "https://testshop1" });
        dbContext.SaveChanges();
    }

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder =>
        optionsBuilder.UseNpgsql($"Host=localhost;Database={DataBase};Username=postgres;Password=P@ssw0rd;"));
    }
}

