using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.ShopApiFactory;

public class ShopApiConfigurationWebAppFactory<TTestDbContainer>
    (string connectionStringSection, string database, int dbPort, string user, string password,
    int httpPort,
    int httpsPort,
    TestServer signalRServer,
     TTestDbContainer? testDbContainer = null)
    : DbApiAPIKestrelConfigurationContainerWebAppFactory<ShopAPIProgram, AlchemyContext, TTestDbContainer>
    (connectionStringSection, database, dbPort, user, password, httpPort, httpsPort, testDbContainer)
    where TTestDbContainer : class, ITestDbContainer, new()
{
    private readonly TestServer _signalRServer = signalRServer;

    private readonly List<Product.Data.Shop> _initialShops = [];

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    protected override void FillTestData(AlchemyContext dbContext)
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
}