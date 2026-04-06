using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Alchemist.Test.Server.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.DBApiWebAppFactory.Context;

public abstract class DBContextAPIKestrelWebAppFactory<TEntryPoint, TDbContext>(bool ensureDeleted, int httpPort, int httpsPort) 
    : DbContextAPIWebAppFactory<TEntryPoint, TDbContext>(ensureDeleted)
     where TEntryPoint : class
    where TDbContext : DbContext
{
    public int HttpPort { get; set; } = httpPort;

    public int HttpsPort { get; set; } = httpsPort;

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        context.Configuration.SetKestrelLocalhostPortsConfig(HttpPort, HttpsPort);
    }
}