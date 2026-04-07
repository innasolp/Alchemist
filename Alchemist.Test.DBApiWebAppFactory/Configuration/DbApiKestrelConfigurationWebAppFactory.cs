using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.DBApiWebAppFactory.Configuration;

public abstract class DbApiKestrelConfigurationWebAppFactory<TEntryPoint, TDbContext>(int httpPort, int httpsPort) 
    : DbConfigurationWebAppFactory<TEntryPoint, TDbContext>
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