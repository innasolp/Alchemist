using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Alchemist.Test.Server.Fixtures;

namespace Alchemist.Test.DBApiWebAppFactory.Context;

public abstract class DbApiAPIKestrelContextContainerWebAppFactory<TEntryPoint, TDbContext>(bool ensureDeleted, int httpPort, int httpsPort) 
    : DbContextAPIWebAppFactory<TEntryPoint, TDbContext>(ensureDeleted)
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