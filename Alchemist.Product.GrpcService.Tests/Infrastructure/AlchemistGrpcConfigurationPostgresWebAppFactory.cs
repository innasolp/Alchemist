using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Server.Fixtures;
using Test.PostresqlTestContainer;

namespace Alchemist.Product.GrpcService.Tests.Infrastructure;

internal class GrpcConfigurationDbInterceptor(IWebHostConfigure webHostConfigure,
    string connectionStringSection,
    string database,
    string user,
    string password,
    int port,
    PostgresqlTestDbContainer? testDbContainer = null,
    PostgresDbRespawner? dbRespawner = null)
    : DbConfigurationContainerWebAppInterceptor<AlchemyContext, PostgresqlTestDbContainer, PostgresDbRespawner>
    (webHostConfigure, connectionStringSection, database, user, password, port, testDbContainer, dbRespawner)
{
    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Brands.Add(new Brand { Name = "Elizavecca", CountryId = 2, Comment = "korea" });
        dbContext.Brands.Add(new Brand { Name = "infinite" });
        dbContext.Brands.Add(new Brand { Name = "Infinite " });
        dbContext.SaveChanges();
    }
}

public class AlchemistGrpcConfigurationPostgresWebAppFactory : TestWebAppKestrelFactory<GrpcServiceProgramm>, IAsyncLifetime
{
    private readonly GrpcConfigurationDbInterceptor _dbInterceptor;

    public AlchemistGrpcConfigurationPostgresWebAppFactory(string database, int httpPort = 8070, int httpsPort = 8071) : base(httpPort, httpsPort)
    {
        _dbInterceptor = new GrpcConfigurationDbInterceptor(this, "ConnectionStrings:DbContext2", database, "postgres", "P@ssw0rd", 5432);
    }

    public Task InitializeAsync()
    {
        return _dbInterceptor.InitializeAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _dbInterceptor.DisposeAsync();
    }

    public Task ResetDatabaseAsync()
    {
        return _dbInterceptor.ResetDatabaseAsync();
    }
}