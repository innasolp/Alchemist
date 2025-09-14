using Alchemist.Product.SignalR;
using Alchemist.Test.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Alchemist.Test.SignalRWebAppFactory
{
    public class SignalRLogContextWebAppFactory <TLogContext>: WebApplicationFactory<Startup>
        where TLogContext: FixtureLogContext, new()
    {
        public TLogContext FixtureLoggingContext { get; } = new TLogContext();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(FixtureLoggingContext.ConfigureServices);
        }
    }
}
