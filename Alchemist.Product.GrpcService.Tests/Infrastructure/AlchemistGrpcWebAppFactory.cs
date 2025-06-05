using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.Server.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.GrpcService.Tests.Infrastructure;

public class AlchemistGrpcWebAppFactory : DbContextWebAppFactory<Program, AlchemyContext>
{
    public string DataBase { get; set; } = "test_ci_db";

    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Brands.Add(new Brand { Name = "Elizavecca" });
        dbContext.SaveChanges();
    }

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder =>
        optionsBuilder.UseNpgsql($"Host=localhost;Database={DataBase};Username=postgres;Password=P@ssw0rd;"));
    }
}
