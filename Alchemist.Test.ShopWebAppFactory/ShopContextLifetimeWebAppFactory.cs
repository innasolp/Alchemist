using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.ShopApiFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shop.API.Client;
using Shop.Interfaces;
using Xunit;

namespace Alchemist.Test.ShopWebAppFactory;

public abstract class ShopContextLifetimeWebAppFactory(bool isApi, int httpPort, int httpsPort, int shopAPIHttpPort, int shopAPIHttpsPort, TestServer signalRTestServer)
    : TestWebAppKestrelFactory<ShopWebAppProgram>(httpPort, httpsPort), IAsyncLifetime
{
    private readonly bool _isApi = isApi;    

    private HttpClient? _shopApiClient;

    protected abstract Task<string> GetShopDbConnectionString();

    protected virtual async Task<HttpClient> CreateShopApiHttpClient()
    {
        var connectionString = await GetShopDbConnectionString();
        return ShopApiHelper.CreateShopApiClient(connectionString, shopAPIHttpPort, shopAPIHttpsPort, signalRTestServer);    
    }

    public virtual async Task InitializeAsync()
    {
        _shopApiClient = await CreateShopApiHttpClient();   
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (_isApi)
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
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return LifetimeDisposeAsync();
    }
}