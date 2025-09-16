using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Test.Server.Fixtures;

public abstract class TestWebAppFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
     where TEntryPoint : class
{
    protected abstract void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services);    

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(ConfigureWebHostBuilderContext);
    }
}
