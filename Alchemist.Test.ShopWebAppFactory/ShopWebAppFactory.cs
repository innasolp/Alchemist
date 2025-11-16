using Alchemist.DataService.Interfaces;
using Alchemist.Product.RestAPIClient;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.DependencyInjection;


namespace Alchemist.Test.ShopWebAppFactory;

public class ShopWebAppFactory(bool isApi, int httpPort, int httpsPort, HttpClient shopApiClient) : TestWebAppKestrelFactory<ShopWebAppProgram>(httpPort, httpsPort)
{
    private readonly bool _isApi = isApi;

    public HttpClient ShopApiClient { get; } = shopApiClient;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (_isApi)
            builder.UseSetting("api", "true");

        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.InterceptImplementation<IShopDataService, ShopApiClient>(new ShopApiClient(ShopApiClient));
    }
}
