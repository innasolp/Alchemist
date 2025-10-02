using Alchemist.DataService.Interfaces;
using Alchemist.Product.RestAPIClient;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.ShopWebApp.IntegratonTest.Infrastructure;

public abstract class ShopWebAppFactory : TestWebAppKestrelFactory<ShopWebAppProgram>
{
    private readonly bool _isApi;

    private readonly ShopAPIWebAppFactory _shopAPIWebAppFactory;

    private readonly SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> _signalRApplicationFactory;

    public TestServer SignalRTestServer => _signalRApplicationFactory.Server;

    public HttpClient ShopApiClient { get; }

    public ShopWebAppFactory(bool isApi, string connectionSection, int httpPort, int httpsPort,
        int shopAPIHttpPort, int shopAPIHttpsPort) : base(httpPort, httpsPort)
    {
        _isApi = isApi;

        var settings = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

        var alchemyDbConnectionString = settings.GetConnectionString(connectionSection);

        _signalRApplicationFactory = new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>();
        _signalRApplicationFactory.CreateClient();

        _shopAPIWebAppFactory = new ShopAPIWebAppFactory(alchemyDbConnectionString, _signalRApplicationFactory.Server, shopAPIHttpPort, shopAPIHttpsPort);
        ShopApiClient = _shopAPIWebAppFactory.CreateClient();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if(_isApi)
          builder.UseSetting("api", "true");
        
        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.InterceptImplementation<IShopDataService, ShopApiClient>(new ShopApiClient(ShopApiClient));
    }
}
