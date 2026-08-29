using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Test.Server.Fixtures;

public abstract class TestWebAppFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IWebHostBuilderConfigure, IServices
     where TEntryPoint : class
{
    private Action<WebHostBuilderContext, IConfigurationBuilder>? _configureAppConfiguration;

    public event Action<WebHostBuilderContext, IConfigurationBuilder> ConfigureAppConfiguration
    {
        add
        {
            _configureAppConfiguration += value;
        }
        remove
        {
            _configureAppConfiguration -= value;
        }
    }

    private Action<WebHostBuilderContext, IServiceCollection>? _configureWebHostBuilderContextServices;

    public event Action<WebHostBuilderContext, IServiceCollection> ConfigureWebHostBuilderContextServices
    {
        add
        {
            _configureWebHostBuilderContextServices += value;
        }
        remove
        {
            _configureWebHostBuilderContextServices -= value;
        }
    }

    protected virtual void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        _configureWebHostBuilderContextServices?.Invoke(context, services);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(ConfigureApp);

        base.ConfigureWebHost(builder);

        builder.ConfigureServices(ConfigureWebHostBuilderContext);
    }

    protected virtual void ConfigureApp(WebHostBuilderContext context, IConfigurationBuilder config)
    {
        _configureAppConfiguration?.Invoke(context, config);
    }
}