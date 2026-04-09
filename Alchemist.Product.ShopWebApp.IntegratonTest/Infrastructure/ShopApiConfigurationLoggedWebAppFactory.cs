using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.ShopWebAppFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;

namespace Alchemist.Product.ShopWebApp.IntegratonTest.Infrastructure;

public class ShopApiConfigurationLoggedWebAppFactory : ShopApiClientConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner>, ILoggedContext    
{
    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public ShopApiConfigurationLoggedWebAppFactory() 
        : base(true, 8406, 8407, 8064, 8065, 
            new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server, 
            "ConnectionStrings:DbContext2",
            Common.ConfigurationHelper.GetSectionValue("ShopWebApiLogTestDb"))
    {
    }

    protected ShopApiConfigurationLoggedWebAppFactory(bool isApi,
        int httpPort,
    int httpsPort,
    int shopAPIHttpPort,
    int shopAPIHttpsPort,
    string shopDbConnectionStringSection,
    string shopDatabase)
        : base(isApi, httpPort, httpsPort, shopAPIHttpPort, shopAPIHttpsPort,
            new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server,
            shopDbConnectionStringSection,
            shopDatabase)
    {
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}