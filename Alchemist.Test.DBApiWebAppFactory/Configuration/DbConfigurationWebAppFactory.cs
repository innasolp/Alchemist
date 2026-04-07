using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Test.DBApiWebAppFactory.Configuration;

public abstract class DbConfigurationWebAppFactory<TEntryPoint, TDbContext>
    : TestWebAppFactory<TEntryPoint>
    where TEntryPoint : class
    where TDbContext : DbContext
{
    protected abstract string ConnectionString { get; }

    protected abstract string ConnectionStringSection { get; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection([new KeyValuePair<string,string?> (ConnectionStringSection, ConnectionString)]);
        });

        base.ConfigureWebHost(builder);
    }

    protected abstract void FillTestData(TDbContext dbContext);

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = builder.Build();

        host.Start();

        using var scope = host.Services.CreateScope();
        ConfigureServiceProvider(scope.ServiceProvider);

        return host;
    }

    protected virtual void ConfigureServiceProvider(IServiceProvider serviceProvider)
    {
        using var appContext = serviceProvider.GetRequiredService<TDbContext>();
        try
        {
            FillTestData(appContext);
        }
        catch
        {
            throw;
        }
    }
}