using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Test.Server.Fixtures;

public abstract class DbContextWebAppFactory<TEntryPoint, TDbContext> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
    where TDbContext : DbContext
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(ConfigureServices);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = builder.Build();

        host.Start();

        using var scope = host.Services.CreateScope();
        ConfigureServiceProvider(scope.ServiceProvider);

        return host;
    }

    protected abstract IServiceCollection AddDbContext(IServiceCollection services);

    protected abstract void FillTestData(TDbContext dbContext);

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        AddDbContext(services);
    }

    protected virtual void ConfigureServiceProvider(IServiceProvider serviceProvider)
    {
        using var appContext = serviceProvider.GetRequiredService<TDbContext>();
        try
        {
            appContext.Database.EnsureDeleted();
            appContext.Database.EnsureCreated();

            FillTestData(appContext);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}