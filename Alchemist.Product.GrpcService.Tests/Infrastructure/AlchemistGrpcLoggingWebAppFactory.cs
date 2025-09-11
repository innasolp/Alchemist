using Alchemist.Test.Log;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.GrpcService.Tests.Infrastructure;

public class AlchemistGrpcLoggingWebAppFactory: AlchemistGrpcWebAppFactory
{
    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}
