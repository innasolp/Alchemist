using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Test.DBApiWebAppFactory.Configuration;

public abstract class DbApiKestrelConfigurationWebAppFactory<TEntryPoint, TDbContext>(int httpPort, int httpsPort) 
    : DbConfigurationWebAppFactory<TEntryPoint, TDbContext>
     where TEntryPoint : class
    where TDbContext : DbContext
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
}