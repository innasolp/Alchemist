using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;
using Test.DbContainer.Abstractions;
using Alchemist.Test.Server.Fixtures;
using Xunit;

namespace Alchemist.Test.ShopApiFactory;


public class ShopApiConfigurationWebAppFactory<TTestDbContainer, TDbRespawner> : TestWebAppKestrelFactory<ShopAPIProgram>, IAsyncLifetime
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
{
    private readonly TestServer _signalRServer;

    private readonly DbConfigurationContainerWebAppInterceptor<AlchemyContext, TTestDbContainer, TDbRespawner> _dbInterceptor;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    private readonly List<Product.Data.Shop> _initialShops = [];

    public ShopApiConfigurationWebAppFactory(string connectionStringSection, string database, int dbPort, string user, string password,
    int httpPort,
    int httpsPort,
    TestServer signalRServer,
     TTestDbContainer? testDbContainer = null,
     TDbRespawner? dbRespawner = null,
     Action<AlchemyContext>? fillTestData = null) : base
    (httpPort, httpsPort)
    {
        _signalRServer = signalRServer;

        _dbInterceptor = new DbConfigurationContainerWebAppInterceptor<AlchemyContext, TTestDbContainer, TDbRespawner>(this,
            connectionStringSection, 
            database,
            user, 
            password,
            dbPort,
            testDbContainer,
            dbRespawner,
            fillTestData ?? FillTestData);
    }
    
    protected virtual void FillTestData(AlchemyContext dbContext)
    {
        _initialShops.AddRange(ShopTestRepository.CreateShopsTestData(4));
        _initialShops.ForEach(s => dbContext.Shops.Add(s));
        dbContext.SaveChanges();
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.SetSignalRHubTestSender(_signalRServer, ["events"]);

        FixtureLoggingContext.ConfigureServices(services);
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