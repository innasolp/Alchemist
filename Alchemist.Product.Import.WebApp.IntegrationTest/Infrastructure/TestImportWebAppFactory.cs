using Alchemist.DataService.Interfaces;
using Alchemist.Product.RestAPIClient;
using Alchemist.Product.SignalR;
using Alchemist.Settings.RestAPIClient;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Import.WebApp.IntegrationTest.Infrastructure;

public class TestImportWebAppFactory : TestWebAppFactory<ImportWebAppProgram>
{
    private readonly SettingsAPIWebAppFactory _settingsAPIWebAppFactory;
    private readonly ShopAPIWebAppFactory _shopAPIWebAppFactory;

    private readonly WebApplicationFactory<Startup> _signalRApplicationFactory;

    private readonly HttpClient _shopApiClient;
    private readonly HttpClient _settingsApiClient;

    public TestImportWebAppFactory()
    {
        _signalRApplicationFactory = new WebApplicationFactory<Startup>();
        _signalRApplicationFactory.CreateClient();

        _shopAPIWebAppFactory = new ShopAPIWebAppFactory(_signalRApplicationFactory.Server);
        _shopApiClient = _shopAPIWebAppFactory.CreateClient();

        _settingsAPIWebAppFactory = new SettingsAPIWebAppFactory();
        _settingsApiClient = _settingsAPIWebAppFactory.CreateClient();
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    { 
        services.InterceptImplementation<IShopDataService, ShopApiClient>(new ShopApiClient(_shopApiClient));
        services.InterceptImplementation<IShopSettingsDataService, SettingsAPIClient>(new SettingsAPIClient(_settingsApiClient));
    }
}
