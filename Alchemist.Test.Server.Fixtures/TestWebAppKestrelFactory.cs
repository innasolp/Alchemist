using Microsoft.AspNetCore.Hosting;

namespace Alchemist.Test.Server.Fixtures;

public class TestWebAppKestrelFactory<TEntryPoint>(int httpPort, int httpsPort) : TestHostServerWebAppFactory<TEntryPoint>
     where TEntryPoint : class
{
    public int HttpPort { get; set; } = httpPort;

    public int HttpsPort { get; set; } = httpsPort;

    protected override void ConfigureHostAdresses(IWebHostBuilder builder)
    {
        builder.UseKestrel();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            context.Configuration.SetKestrelLocalhostPortsConfig(HttpPort, HttpsPort);
        });
    }    
}
