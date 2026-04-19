using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.DBApiWebAppFactory.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;

namespace Alchemist.Product.GrpcService.Tests.Infrastructure;

public class AlchemistGrpcContextWebAppFactory : DbContextWebAppFactory<GrpcServiceProgramm, AlchemyContext>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    private readonly string _host = Guid.NewGuid().ToString();

    public AlchemistGrpcContextWebAppFactory()
    {
        _postgreSqlContainer = PostgresqlTestContainerHelper.BuildPostgreSqlContainer(_host);
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Brands.Add(new Brand { Name = "Elizavecca", CountryId= 2, Comment = "korea" });
        dbContext.Brands.Add(new Brand { Name = "infinite" });
        dbContext.Brands.Add(new Brand { Name = "Infinite " });
        dbContext.SaveChanges();
    }    

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddAlchemyPostgresContextFactory((sp,optionsBuilder) =>
            optionsBuilder.UseNpgsql(_postgreSqlContainer.BuildConnectionString(DataBase)));
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {        
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