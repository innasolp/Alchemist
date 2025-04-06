using Alchemist.Product.Data;
using Alchemist.Test.Server.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.GrpcService.Tests;

public class AlchemistGrpcWebAppFactory : AlchemistWebAppFactory<Program, AlchemyContext>
{    
    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Brands.Add(new Brand { Name = "Elizavecca" });
        dbContext.SaveChanges();
    }

    protected override void RegisterServices(IServiceCollection services)
    {
        //todo
    }

    protected override DbContextOptionsBuilder SetDbContext(DbContextOptionsBuilder optionsBuilder)
    {
        return optionsBuilder.UseNpgsql("Host=localhost;Database=test_ci_db;Username=postgres;Password=P@ssw0rd;");
    }    
}
