using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Shop.API.Test.Infrastructure;

public class ShopAPISignalRWebAppFactory(TestServer signalRServer) : ShopAPIContextWebAppFactory
{
    private readonly TestServer _signalRServer = signalRServer;

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.SetSignalRHubTestSender(_signalRServer, ["events"]);
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {        
    }
}
