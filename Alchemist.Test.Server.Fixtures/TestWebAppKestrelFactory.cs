using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Test.Server.Fixtures;

public class TestWebAppKestrelFactory<TEntryPoint>(int httpPort, int httpsPort) : TestHostServerWebAppFactory<TEntryPoint>
     where TEntryPoint : class
{
    public int HttpPort { get; set; } = httpPort;

    public int HttpsPort { get; set; } = httpsPort;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((context, config) =>
        {
            context.Configuration.SetKestrelLocalhostPortsConfig(HttpPort, HttpsPort);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.CreateTestHostUseAddressConfiguration(ConfigureHostAdresses, out _host);

        ClientOptions.BaseAddress = _host.GetBaseAddress();

        ConfigureHost(_host);

        return testHost;
    }
}
