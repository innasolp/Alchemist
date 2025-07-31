using Alchemist.Product.Data;
using Alchemist.Product.Interfaces;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class SettingsAPIWebAppFactory (string connectionString) : DbAPIWebAppFactory<SettingsAPIProgram, AlchemyContext>(true)
{
    private readonly string _connectionString = connectionString;    

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder => optionsBuilder.UseNpgsql(_connectionString));
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
        var shopSettings = TestRepository.GetShopSettingsImportTestData(dbContext.Shops);
        shopSettings.ForEach(s => dbContext.ShopSettings.Add(s.To<ShopSettings>()));
        dbContext.SaveChanges();

        var serviceSettings = TestRepository.GetShopSettingsServicesTestData(dbContext.ShopSettings);
        serviceSettings.ForEach(s => dbContext.ShopSettings.Add(s.To<ShopSettings>()));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            context.SetKestrelLocalhostPortsConfig(8200, 8201);
        });
    }
}
