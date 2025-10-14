using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Alchemist.Test.DBApiWebAppFactory;
using Alchemist.Test.Log;

namespace Alchemist.Test.ShopApiFactory;

public class ShopAPIWebAppFactory(string connectionString, TestServer signalRServer, int httpPort, int httpsPort, bool ensureDeleted = true) 
    : DBAPIKestrelWebAppFactory<ShopAPIProgram, AlchemyContext>(ensureDeleted, httpPort, httpsPort)
{
    private readonly TestServer _signalRServer = signalRServer;

    private readonly string _connectionString = connectionString;

    public ShopAPIWebAppFactory(string connectionString, TestServer signalRServer, bool ensureDeleted = true)
        : this(connectionString, signalRServer, 8050, 8051, ensureDeleted) { }

    public FixtureLoggerFactoryContext FixtureLoggingContext   { get; } = new FixtureLoggerFactoryContext();

    protected override void FillTestData(AlchemyContext dbContext)
    {
        var shops = ShopTestRepository.CreateShopsTestData(4);
        shops.ForEach(s => dbContext.Shops.Add(s.To<Shop>()));
        dbContext.SaveChanges();
    }

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder => optionsBuilder.UseNpgsql(_connectionString));
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.SetSignalRHubTestSender(_signalRServer, ["events"]);

        FixtureLoggingContext.ConfigureServices(services);
    }
}

