using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.DBApiWebAppFactory.Configuration;

public abstract class DbApiAPIKestrelConfigurationContainerWebAppFactory<TEntryPoint, TDbContext, TTestDbContainer>
    (string connectionStringSection, string database, int dbPort, string user, string password, int httpPort, int httpsPort)
    : DbConfigurationContainerWebAppFactory<TEntryPoint, TDbContext, TTestDbContainer>(connectionStringSection, database, dbPort, user, password)
    where TEntryPoint : class
    where TDbContext : DbContext
    where TTestDbContainer : ITestDbContainer, new()
{
    public int HttpPort { get; set; } = httpPort;

    public int HttpsPort { get; set; } = httpsPort;

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        context.Configuration.SetKestrelLocalhostPortsConfig(HttpPort, HttpsPort);
    }
}