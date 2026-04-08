using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;

namespace Alchemist.Product.Import.DBService.Test;

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
    protected override void FillTestData(AlchemyContext dbContext) {}
}
public class GrpcServiceWebAppFactory : TestWebAppKestrelFactory<GrpcServiceProgramm>, IAsyncLifetime
{
    private readonly GrpcConfigurationDbInterceptor _dbInterceptor;

    public event Action<IServiceCollection>? ConfigureServices;

    public GrpcServiceWebAppFactory(string database, int httpPort = 8070, int httpsPort = 8071) : base(httpPort, httpsPort)
    {
        _dbInterceptor = new GrpcConfigurationDbInterceptor(this, "ConnectionStrings:DbContext2", database, "postgres", "P@ssw0rd", 5432);
    }
    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        ConfigureServices?.Invoke(services);
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