
using Alchemist.Test.Server.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.GrpcService.Tests;

public class AlchemistGrpcLoggingWebAppFactory: AlchemistGrpcWebAppFactory
{
    public FixtureLoggingContext FixtureLoggingContext { get; } = new FixtureLoggingContext();

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        FixtureLoggingContext.ConfigureServices(services);
    }
}
