using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Alchemist.Test.SignalRWebAppFactory;

public class SignalRLogContextWebAppFactory <TLogContext>: SignalRApplicationFactory
    where TLogContext: FixtureLogContext, new()
{
    public TLogContext FixtureLoggingContext { get; } = new TLogContext();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(FixtureLoggingContext.ConfigureServices);
    }
}
