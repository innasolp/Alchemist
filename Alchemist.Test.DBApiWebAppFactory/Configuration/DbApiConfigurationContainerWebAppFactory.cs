using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.DBApiWebAppFactory.Configuration;

public abstract class DbApiConfigurationContainerWebAppFactory<TEntryPoint, TDbContext, TTestDbContainer>
    (string connectionStringSection, string database, int port, string user, string password)
    : DbConfigurationContainerWebAppFactory<TEntryPoint, TDbContext, TTestDbContainer>(connectionStringSection, database, port, user, password)
    where TEntryPoint : class
    where TDbContext : DbContext
    where TTestDbContainer : ITestDbContainer, new()
{
    private IHost? _host;

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