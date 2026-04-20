using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Test.DBApiWebAppFactory.Configuration;

public abstract class DbConfigurationWebAppInterceptor<TDbContext> : IDisposable
    where TDbContext : DbContext
{
    private readonly IWebHostConfigure _webHostConfigure;

    protected abstract string ConnectionString { get; }

    protected abstract string ConnectionStringSection { get; }

    public DbConfigurationWebAppInterceptor(IWebHostConfigure webHostConfigure)
    {
        _webHostConfigure = webHostConfigure;

        _webHostConfigure.ConfigureAppConfiguration += ConfigureAppConfiguration;
        _webHostConfigure.ConfigureWebHostBuilderContextServices += ConfigureWebHostBuilderContext;
        _webHostConfigure.ConfigureHost += ConfigureHost;
    }

    private void ConfigureHost(IHost host)
    {
        using var scope = host.Services.CreateScope();
        ConfigureServiceProvider(scope.ServiceProvider);
    }
    protected virtual void ConfigureServiceProvider(IServiceProvider serviceProvider)
    {
        using var appContext = serviceProvider.GetRequiredService<TDbContext>();

        appContext.Database.EnsureCreated();

        FillTestData(appContext);
    }

    protected abstract void FillTestData(TDbContext dbContext);

    protected virtual void ConfigureWebHostBuilderContext(WebHostBuilderContext webHostBuilderContext, IServiceCollection services)
    {}

    public void Dispose()
    {
        _webHostConfigure.ConfigureAppConfiguration -= ConfigureAppConfiguration;
        _webHostConfigure.ConfigureWebHostBuilderContextServices -= ConfigureWebHostBuilderContext;
        _webHostConfigure.ConfigureHost -= ConfigureHost;
    }

    private void ConfigureAppConfiguration(WebHostBuilderContext webHostBuilderContext, IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddInMemoryCollection([new KeyValuePair<string, string?>(ConnectionStringSection, ConnectionString)]);
    }
}