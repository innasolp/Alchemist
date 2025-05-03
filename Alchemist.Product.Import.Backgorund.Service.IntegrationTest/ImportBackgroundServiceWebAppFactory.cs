using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceWebAppFactory : WebApplicationFactory<ImportBackgroundServiceProgram>
{
    private readonly SettingsAPIWebAppFactory _settingsAPIWebAppFactory;

    private readonly ShopAPIWebAppFactory _shopAPIWebAppFactory;

    private readonly GrpcServiceWebAppFactory _grpcWebAppFactory;

    public ImportBackgroundServiceWebAppFactory()
    {
        var settings = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

        var alchemyDbConnectionString = settings.GetConnectionString("alchemydb");

        _shopAPIWebAppFactory = new ShopAPIWebAppFactory(alchemyDbConnectionString);
        _shopAPIWebAppFactory.CreateClient();

        _settingsAPIWebAppFactory = new SettingsAPIWebAppFactory(alchemyDbConnectionString);
        _settingsAPIWebAppFactory.CreateClient();

        _grpcWebAppFactory = new GrpcServiceWebAppFactory(alchemyDbConnectionString);  
    }    
}
