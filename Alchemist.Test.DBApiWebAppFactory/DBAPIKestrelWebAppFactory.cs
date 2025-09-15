using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Alchemist.Test.Server.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.DBApiWebAppFactory;

public abstract class DBAPIKestrelWebAppFactory<TEntryPoint, TDbContext>(bool ensureDeleted, int httpPort, int httpsPort) 
    : DbAPIWebAppFactory<TEntryPoint, TDbContext>(ensureDeleted)
     where TEntryPoint : class
    where TDbContext : DbContext
{
    public int HttpPort { get; set; } = httpPort;

    public int HttpsPort { get; set; } = httpsPort;

    protected virtual void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        context.Configuration.SetKestrelLocalhostPortsConfig(HttpPort, HttpsPort);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(ConfigureWebHostBuilderContext);
    }
}
