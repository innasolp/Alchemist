using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;

namespace Alchemist.Product.Import.DBService.Test;

public class GrpcServiceWebAppFactory : TestWebAppKestrelFactory<GrpcServiceProgramm>, IAsyncLifetime
{
    private readonly DbConfigurationContainerWebAppInterceptor<AlchemyContext, PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbChecker> _dbInterceptor;

    public event Action<IServiceCollection>? ConfigureServices;

    public GrpcServiceWebAppFactory(string database, int httpPort = 8070, int httpsPort = 8071) : base(httpPort, httpsPort)
    {
        _dbInterceptor = new DbConfigurationContainerWebAppInterceptor<AlchemyContext, PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbChecker>
            (this, "ConnectionStrings:DbContext2", database, "postgres", "P@ssw0rd", 5432);
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

    public Task ResetDatabaseIfAvailableAsync()
    {
        return _dbInterceptor.ResetDatabaseIfAvailableAsync();
    }
}