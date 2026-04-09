using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shop.API.Client;
using Shop.Interfaces;
using Xunit;

namespace Alchemist.Test.ShopWebAppFactory;

public abstract class ShopApiClientWebAppFactory(bool isApi, int httpPort, int httpsPort, int shopAPIHttpPort, int shopAPIHttpsPort, TestServer signalRTestServer)
    : TestWebAppKestrelFactory<ShopWebAppProgram>(httpPort, httpsPort), IAsyncLifetime
{
    private HttpClient? _shopApiClient;

    protected abstract TestHostServerWebAppFactory<ShopAPIProgram> CreateShopApiFactory(int shopAPIHttpPort, int shopAPIHttpsPort, TestServer signalRTestServer);

    private TestHostServerWebAppFactory<ShopAPIProgram>? _shopApiFactory;

    public virtual async Task InitializeAsync()
    {
        _shopApiFactory = CreateShopApiFactory(shopAPIHttpPort, shopAPIHttpsPort, signalRTestServer);

        if (_shopApiFactory is IAsyncLifetime asyncLifetimeFactory)        
            await asyncLifetimeFactory.InitializeAsync();

        _shopApiClient = _shopApiFactory.GetHostHttpClient(); 
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (isApi)
            builder.UseSetting("api", "true");

        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.InterceptImplementation<IShopDataService, ShopApiClient>(new ShopApiClient(_shopApiClient));
    }

    protected virtual async Task LifetimeDisposeAsync()
    {
        _shopApiClient?.Dispose();

        if (_shopApiFactory is IAsyncLifetime asyncLifetimeFactory)
            await asyncLifetimeFactory.DisposeAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return LifetimeDisposeAsync();
    }
}