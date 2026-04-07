using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;

namespace Alchemist.Test.ShopApiFactory;

public abstract class ShopApiConfigurationWebAppFactory(TestServer signalRServer,
    int httpPort,
    int httpsPort)
    : DbConfigurationApiKestrelWebAppFactory<ShopAPIProgram, AlchemyContext>(httpPort, httpsPort)
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