using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.DBApiWebAppFactory;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class SettingsAPIWebAppFactory (string connectionString, TestServer signalRServer) : DbAPIWebAppFactory<SettingsAPIProgram, AlchemyContext>(true)
{
    private readonly string _connectionString = connectionString;

    private readonly TestServer _signalRServer = signalRServer;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();    

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder => optionsBuilder.UseNpgsql(_connectionString));
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            context.SetKestrelLocalhostPortsConfig(8200, 8201);

            services.SetSignalRTestSender(_signalRServer, ["events"]);

            FixtureLoggingContext.ConfigureServices(services);
        });
    }
}
