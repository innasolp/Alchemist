using Alchemist.Product.SignalR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class SignalRApplicationFactory : WebApplicationFactory<Startup>
{
    protected override IHostBuilder? CreateHostBuilder()
    {
        return base.CreateHostBuilder();
    }
}
