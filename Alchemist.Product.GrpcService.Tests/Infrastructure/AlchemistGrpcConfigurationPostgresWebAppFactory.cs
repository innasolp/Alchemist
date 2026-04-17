using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Server.Fixtures;
using Test.PostresqlTestContainer;

namespace Alchemist.Product.GrpcService.Tests.Infrastructure;

public class AlchemistGrpcConfigurationPostgresWebAppFactory : TestWebAppKestrelFactory<GrpcServiceProgramm>, IAsyncLifetime
{
    private readonly DbConfigurationContainerWebAppInterceptor<AlchemyContext, PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbHelper> _dbInterceptor;

    public AlchemistGrpcConfigurationPostgresWebAppFactory(string database, int httpPort = 8070, int httpsPort = 8071) : base(httpPort, httpsPort)
    {
        _dbInterceptor = new DbConfigurationContainerWebAppInterceptor<AlchemyContext, PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbHelper>
            (this, "ConnectionStrings:DbContext2", database, "postgres", "P@ssw0rd", 5432, fillTestData: FillTestData);
    }

    protected static void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Brands.Add(new Brand { Name = "Elizavecca", CountryId = 2, Comment = "korea" });
        dbContext.Brands.Add(new Brand { Name = "infinite" });
        dbContext.Brands.Add(new Brand { Name = "Infinite " });
        dbContext.SaveChanges();
    }

    public Task InitializeAsync()
    {
        return _dbInterceptor.InitializeAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _dbInterceptor.DisposeAsync();
    }

    public Task ResetDatabaseIfAvailableAsync()
    {
        return _dbInterceptor.ResetDatabaseIfAvailableAsync();
    }
}