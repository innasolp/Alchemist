using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Alchemist.Test.DBApiWebAppFactory;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ShopAPIWebAppFactory(string connectionString, TestServer signalRServer) : DbAPIWebAppFactory<ShopAPIProgram, AlchemyContext>(true)
{
    private readonly TestServer _signalRServer = signalRServer;

    private readonly string _connectionString = connectionString;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    protected override void FillTestData(AlchemyContext dbContext)
    {
    }

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder => optionsBuilder.UseNpgsql(_connectionString));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            context.SetKestrelLocalhostPortsConfig(8050, 8051);

            services.SetSignalRTestSender(_signalRServer, ["events"]);

            FixtureLoggingContext.ConfigureServices(services);
        });
    }
}

