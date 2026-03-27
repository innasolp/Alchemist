using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.DBApiWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;
using Alchemist.Test.Log;

namespace Alchemist.Test.SettingsAPIFactory;

public class SettingsAPIWebAppFactory(string connectionString, TestServer signalRServer, int httpPort, int httpsPort, bool ensureDeleted = true) 
    : DBAPIKestrelWebAppFactory<SettingsAPIProgram, AlchemyContext>(ensureDeleted, httpPort, httpsPort)
{
    private readonly string _connectionString = connectionString;

    private readonly TestServer _signalRServer = signalRServer;

    
    public SettingsAPIWebAppFactory(string connectionString, TestServer signalRServer, bool ensureDeleted = true)
        :this(connectionString, signalRServer, 8200, 8201, ensureDeleted) { }

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();    

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContextPostgres>(optionsBuilder => optionsBuilder.UseNpgsql(_connectionString));
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.SetSignalRHubTestSender(_signalRServer, ["events"]);

        FixtureLoggingContext.ConfigureServices(services);
    }
}
