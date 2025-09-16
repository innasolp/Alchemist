using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Alchemist.Test.DBApiWebAppFactory;
using Alchemist.Test.Log;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;

public class ShopAPIWebAppFactory(string connectionString, TestServer signalRServer, int httpPort, int httpsPort) 
    : DBAPIKestrelWebAppFactory<ShopAPIProgram, AlchemyContext>(true, httpPort, httpsPort)
{
    private readonly TestServer _signalRServer = signalRServer;

    private readonly string _connectionString = connectionString;

    public ShopAPIWebAppFactory(string connectionString, TestServer signalRServer)
        : this(connectionString, signalRServer, 8050, 8051) { }

    public FixtureLoggerFactoryContext FixtureLoggingContext   { get; } = new FixtureLoggerFactoryContext();

    protected override void FillTestData(AlchemyContext dbContext)
    {
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

