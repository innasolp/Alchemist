using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.Server.Fixtures;

public abstract class AlchemistWebAppFactory<TEntryPoint, TDbContext> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
    where TDbContext : DbContext
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            ConfigureServices(services);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            ConfigureServiceProvider(scope.ServiceProvider);
        });
    }

    protected abstract DbContextOptionsBuilder SetDbContext(DbContextOptionsBuilder optionsBuilder);

    protected abstract void FillTestData(TDbContext dbContext);

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddDbContextPool<TDbContext>(optionsBuilder => SetDbContext(optionsBuilder));
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