using Alchemist.Product.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ShopImport.Test.DbApiWebAppFactory.Postgresql;
using Test.PostresqlTestContainer;

namespace Alchemist.Product.GrpcService.Tests.Infrastructure;

public class AlchemistGrpcConfigurationPostgresWebAppFactory(string database)
    : DbApiConfigurationContainerWebAppFactory<GrpcServiceProgramm, AlchemyContext, PostgresqlTestDbContainer>("ConnectionStrings:DbContext2", database, 5432, "postgres", "P@ssw0rd")
{
    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {}

    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Brands.Add(new Brand { Name = "Elizavecca", CountryId = 2, Comment = "korea" });
        dbContext.Brands.Add(new Brand { Name = "infinite" });
        dbContext.Brands.Add(new Brand { Name = "Infinite " });
        dbContext.SaveChanges();
    }
}