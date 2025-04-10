using Alchemist.Product.RestAPI.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.RestAPI.Test;

public class ShopAPILoggingWebAppFactory: ShopAPISignlRMockWebAppFactory
{
    public PerfomanceCounterMiddlewareFixtureLogContext<ShopController> FixtureLoggingContext { get; }
        = new PerfomanceCounterMiddlewareFixtureLogContext<ShopController>();

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}
