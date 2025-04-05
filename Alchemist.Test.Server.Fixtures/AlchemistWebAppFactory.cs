using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Alchemist.Test.Server.Fixtures;

public abstract class AlchemistWebAppFactory<TEntryPoint, TDbContext> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : Program
    where TDbContext : DbContext
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<TDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryAlchemistTest");
                });

            RegisterServices(services);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            using var appContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
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
        });
    }

    protected abstract void FillTestData(TDbContext dbContext);

    protected abstract void RegisterServices(IServiceCollection services);
}