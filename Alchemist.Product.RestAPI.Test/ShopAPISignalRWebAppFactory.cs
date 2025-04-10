using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.RestAPI.Test;

public class ShopAPISignalRWebAppFactory(TestServer signalRServer) : ShopAPIWebAppFactory
{
    private readonly TestServer _signalRServer = signalRServer;

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.SetSignalRTestSender(_signalRServer, ["events"]);
    }
}
