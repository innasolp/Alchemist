using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.Interfaces;
using Alchemist.Test.DBApiWebAppFactory;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Import.WebApp.IntegrationTest.Infrastructure;

public class SettingsAPIWebAppFactory : DbAPIWebAppFactory<SettingsAPIProgram, AlchemyContext>
{
    public SettingsAPIWebAppFactory() : base(false)
    {
    }

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder =>
        optionsBuilder.UseNpgsql("Host=localhost;Database=test_ci_db_importwebapp;Username=postgres;Password=P@ssw0rd;"));
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
        var shopSettings = TestRepository.GetShopSettingsImportTestData(dbContext.Shops);
        shopSettings.ForEach(s => dbContext.ShopSettings.Add(s.To<ShopSettings>()));
        dbContext.SaveChanges();

        var serviceSettings = TestRepository.GetShopSettingsServicesTestData(dbContext.ShopSettings);
        serviceSettings.ForEach(s => dbContext.ShopSettings.Add(s.To<ShopSettings>()));
        dbContext.SaveChanges();
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
