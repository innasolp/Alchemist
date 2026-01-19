using Microsoft.Extensions.DependencyInjection;
using Shop.API.Controllers;

namespace Shop.API.Test.Infrastructure;

public class ShopAPILoggingWebAppFactory: ShopAPISignlRMockWebAppFactory
{
    public MiddlewareFixtureLogContext<ShopController> FixtureLoggingContext { get; }
        = new MiddlewareFixtureLogContext<ShopController>();

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}
