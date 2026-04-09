using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.ShopWebApp.IntegratonTest.Infrastructure;

public class ShopContextApiLoggedWebAppFactory : ShopContextTestContainerLifetimeWebAppFactory, ILoggedContext
{
    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public ShopContextApiLoggedWebAppFactory() : base(true, 8406, 8407, 8064, 8065, Common.ConfigurationHelper.GetSectionValue("ShopWebApiLogTestDb"))
    {
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {        
        base.ConfigureWebHostBuilderContext(context, services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}