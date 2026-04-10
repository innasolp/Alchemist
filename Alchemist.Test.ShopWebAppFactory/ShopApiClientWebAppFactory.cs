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

    protected TestHostServerWebAppFactory<ShopAPIProgram>? ShopApiFactory { get; private set; }

    public virtual async Task InitializeAsync()
    {
        ShopApiFactory = CreateShopApiFactory(shopAPIHttpPort, shopAPIHttpsPort, signalRTestServer);

        if (ShopApiFactory is IAsyncLifetime asyncLifetimeFactory)        
            await asyncLifetimeFactory.InitializeAsync();

        _shopApiClient = ShopApiFactory.GetHostHttpClient(); 
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

        if (ShopApiFactory is IAsyncLifetime asyncLifetimeFactory)
            await asyncLifetimeFactory.DisposeAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return LifetimeDisposeAsync();
    }
}