using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.Server.Fixtures;

public abstract class TestWebAppFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IWebHostBuilderConfigure
     where TEntryPoint : class
{
    private Action<WebHostBuilderContext, IConfigurationBuilder>? _configureAppConfiguration;

    event Action<WebHostBuilderContext, IConfigurationBuilder> IWebHostBuilderConfigure.ConfigureAppConfiguration
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

    private Action<WebHostBuilderContext, IServiceCollection>? _configureWebHostBuilderContext;

    event Action<WebHostBuilderContext, IServiceCollection> IWebHostBuilderConfigure.ConfigureWebHostBuilderContext
    {
        add
        {
            _configureWebHostBuilderContext += value;
        }
        remove
        {
            _configureWebHostBuilderContext -= value;
        }
    }

    protected virtual void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        _configureWebHostBuilderContext?.Invoke(context, services);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            _configureAppConfiguration?.Invoke(context, config);
        });

        base.ConfigureWebHost(builder);

        builder.ConfigureServices(ConfigureWebHostBuilderContext);
    }
}
