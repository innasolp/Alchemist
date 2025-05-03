using Microsoft.AspNetCore.Hosting;

namespace Alchemist.Test.Server.Fixtures;

public class TestWebAppKestrelFactory<TEntryPoint>(int httpPort, int httpsPort) : TestWebAppFactory<TEntryPoint>
     where TEntryPoint : class
{
    public int HttpPort { get; } = httpPort;

    public int HttpsPort { get; } = httpsPort;

    protected override void ConfigureHostAdresses(IWebHostBuilder builder)
    {
        builder.UseKestrel();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            context.SetKestrelLocalhostPortsConfig(HttpPort, HttpsPort);
        });
    }    
}
