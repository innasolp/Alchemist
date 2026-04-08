using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;

namespace Shop.API.Test.Infrastructure;

internal class ShopDbConfigurationContainerWebAppInterceptor(ShopApiConfigurationWebAppFactory webHostConfigure,
    string connectionStringSection,
    string database) 
    : DbConfigurationContainerWebAppInterceptor<AlchemyContext, PostgresqlTestDbContainer, PostgresDbRespawner>
    (webHostConfigure, connectionStringSection, database, "postgres", "P@ssw0rd", 5432)
{
    protected override void FillTestData(AlchemyContext dbContext)
    {
        dbContext.Shops.Add(new Alchemist.Product.Data.Shop { Name = "TestShop", Url = "https://testshop1" });
        dbContext.SaveChanges();
    }
}

public abstract class ShopApiConfigurationWebAppFactory : TestWebAppKestrelFactory<ShopAPIProgram>, IAsyncLifetime
{
    private readonly Action<IServiceCollection>? _configureServices;

    private readonly ShopDbConfigurationContainerWebAppInterceptor _dbInterceptor;

    protected ShopApiConfigurationWebAppFactory(string connectionStringSection, 
    string database, 
    Action<IServiceCollection>? configureServices = null,
    int httpPort = 8050,
    int httpsPort = 8051) : base(httpPort, httpsPort)
    {
        _configureServices = configureServices;

        _dbInterceptor = new ShopDbConfigurationContainerWebAppInterceptor(this, connectionStringSection, database);
    }

    public Task InitializeAsync()
    {
        return _dbInterceptor.InitializeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        if(_configureServices != null)
            builder.ConfigureTestServices(_configureServices);
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