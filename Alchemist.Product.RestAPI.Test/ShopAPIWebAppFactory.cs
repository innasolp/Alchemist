using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.Server.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.RestAPI.Test;

public abstract class ShopAPIWebAppFactory : AlchemistDbContextWebAppFactory<Startup, AlchemyContext>
{
    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Shops.Add(new Shop { Name = "TestShop", Url = "https://testshop1" });
        dbContext.SaveChanges();
    }

    //protected override DbContextOptionsBuilder SetDbContext(DbContextOptionsBuilder optionsBuilder)
    //{
    //    return optionsBuilder.UseNpgsql("Host=localhost;Database=test_ci_db;Username=postgres;Password=P@ssw0rd;");
    //}

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder =>
        optionsBuilder.UseNpgsql("Host=localhost;Database=test_ci_db;Username=postgres;Password=P@ssw0rd;"));
    }
}

