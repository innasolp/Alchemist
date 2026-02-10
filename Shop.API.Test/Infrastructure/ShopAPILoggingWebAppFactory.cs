using Alchemist.Test.Log;
using Microsoft.Extensions.DependencyInjection;

namespace Shop.API.Test.Infrastructure;

public class ShopAPILoggingWebAppFactory: ShopAPISignlRMockWebAppFactory
{
    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}