using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Test.DBApiWebAppFactory;

public abstract class DbAPIWebAppFactory<TEntryPoint, TDbContext>(bool ensureDeleted) : DbContextWebAppFactory<TEntryPoint, TDbContext>
     where TEntryPoint : class
    where TDbContext : DbContext
{
    private readonly bool _ensureDeleted = ensureDeleted;

    private IHost _host;

    public string ServerAddress
    {
        get
        {
            EnsureServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }

    private void EnsureServer()
    {
        if (_host is null)
        {
            // This forces WebApplicationFactory to bootstrap the server  
            using var _ = CreateDefaultClient();
        }
    }

    protected override void ConfigureServiceProvider(IServiceProvider serviceProvider)
    {
        using var appContext = serviceProvider.GetRequiredService<TDbContext>();
        try
        {
            if (_ensureDeleted) appContext.Database.EnsureDeleted();
            appContext.Database.EnsureCreated();

            FillTestData(appContext);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.CreateTestHostUseAddressConfiguration(ConfigureHostAdresses, out _host);

        ClientOptions.BaseAddress = _host.GetBaseAddress();

        using var scope = testHost.Services.CreateScope();
        ConfigureServiceProvider(scope.ServiceProvider);

        return testHost;
    }

    protected virtual void ConfigureHostAdresses(IWebHostBuilder builder)
    {
        builder.UseKestrel();
    }
}
