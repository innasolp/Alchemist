using Alchemist.Product.Data;
using Alchemist.Test.Server.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.RestAPI.Test;

public abstract class ShopAPIWebAppFactory : AlchemistWebAppFactory<Startup, AlchemyContext>
{
    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Shops.Add(new Shop { Name = "TestShop", Url = "https://testshop1" });
        dbContext.SaveChanges();
    }

    protected override DbContextOptionsBuilder SetDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.UseNpgsql("Host=localhost;Database=test_ci_db;Username=postgres;Password=P@ssw0rd;");
    }
}

