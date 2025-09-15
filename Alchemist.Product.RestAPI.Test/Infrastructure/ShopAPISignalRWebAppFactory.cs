using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.RestAPI.Test.Infrastructure;

public class ShopAPISignalRWebAppFactory(TestServer signalRServer) : ShopAPIWebAppFactory
{
    private readonly TestServer _signalRServer = signalRServer;

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.SetSignalRHubTestSender(_signalRServer, ["events"]);
    }
}
