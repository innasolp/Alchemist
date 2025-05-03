using Alchemist.Product.SignalR;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Alchemist.Product.Import.WebApp.IntegrationTest;

public class TestImportWebAppFactory : TestWebAppKestrelFactory<ImportWebAppProgram>
{
    private readonly SettingsAPIWebAppFactory _settingsAPIWebAppFactory;
    private readonly ShopAPIWebAppFactory _shopAPIWebAppFactory;

    private readonly WebApplicationFactory<Startup> _signalRApplicationFactory;    

    public TestImportWebAppFactory() : base(8110,8111)
    {
        _signalRApplicationFactory = new WebApplicationFactory<Startup>();
        _signalRApplicationFactory.CreateClient();

        _shopAPIWebAppFactory = new ShopAPIWebAppFactory(_signalRApplicationFactory.Server);
        _shopAPIWebAppFactory.CreateClient();

        _settingsAPIWebAppFactory = new SettingsAPIWebAppFactory();
        _settingsAPIWebAppFactory.CreateClient();
    }
}
