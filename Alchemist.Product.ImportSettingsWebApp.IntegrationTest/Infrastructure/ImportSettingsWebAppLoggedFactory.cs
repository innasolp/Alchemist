using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;

namespace Alchemist.Product.ImportSettingsWebApp.IntegrationTest.Infrastructure;

public class ImportSettingsWebAppLoggedFactory 
    : ImportSettingsShopClientConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbChecker>, ILoggedContext
{
    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    protected ImportSettingsWebAppLoggedFactory(bool isApi, 
        int httpPort, 
        int httpsPort, 
        int settingsApiHttpPort, 
        int settingsApiHttpsPort, 
        string settingsDbConnectionStringSection = "ConnectionStrings:DbContext",
        string settingsDatabase = "alchemy", 
        string shopDbConnectionStringSection = "ConnectionStrings:DbContext",
        string shopDatabase = "alchemy",
        int? shopApiHttpPort = null, 
        int? shopApiHttpsPort = null, 
        int? shopWebappApiHttpPort = null, int? shopWebAppApiHttpsPort = null) 
        : base(isApi, httpPort, httpsPort, 
            new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server, 
            settingsApiHttpPort, settingsApiHttpsPort, settingsDbConnectionStringSection, settingsDatabase, shopDbConnectionStringSection, shopDatabase, shopApiHttpPort, shopApiHttpsPort, shopWebappApiHttpPort, shopWebAppApiHttpsPort)
    {
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}
