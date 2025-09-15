using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.DBApiWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;
using Alchemist.Test.Log;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest.Infrastructure;

public class SettingsAPIWebAppFactory (string connectionString, TestServer signalRServer, int httpPort, int httpsPort) 
    : DBAPIKestrelWebAppFactory<SettingsAPIProgram, AlchemyContext>(true, httpPort, httpsPort)
{
    private readonly string _connectionString = connectionString;

    private readonly TestServer _signalRServer = signalRServer;
    
    public SettingsAPIWebAppFactory(string connectionString, TestServer signalRServer)
        :this(connectionString, signalRServer, 8200, 8201) { }

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();    

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder => optionsBuilder.UseNpgsql(_connectionString));
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
