using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.Interfaces;
using Alchemist.Test.DBApiWebAppFactory;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;



namespace Alchemist.Settings.RestAPI.Test;

public class SettingsAPIWebAppFactory : DbContextWebAppFactory<SettingsAPIProgram, AlchemyContext>, ILoggedContext
{
    private readonly SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> _signalRApplicationFactory;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();
    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public event Action<WebHostBuilderContext, IServiceCollection> ConfigureContextServices;

    public IShop[] Shops { get; } = new IShop[2];

    public TestServer SignalRTestServer => _signalRApplicationFactory.Server;

    public SettingsAPIWebAppFactory()
    {
        _signalRApplicationFactory = new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>();
        _signalRApplicationFactory.CreateClient();
    }

    public string DataBase { get; set; } = "test_ci_db";

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder =>
        optionsBuilder.UseNpgsql($"Host=localhost;Database={DataBase};Username=postgres;Password=P@ssw0rd;"));
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
        Shops[0] = dbContext.Shops.Add(new Shop { Name = "TestShop1", Url = "https://testshop2" }).Entity;        
        Shops[1] = dbContext.Shops.Add(new Shop { Name = "TestShop1", Url = "https://testshop2" }).Entity;

        dbContext.SaveChanges();        

       var shopSettings = TestRepository.CreateCategoryShopSettings(Shops[0].Id, Shops[0].Name); 

        dbContext.ShopSettings.Add(shopSettings.To<ShopSettings>());
        dbContext.SaveChanges();
        var savedShopSettings = dbContext.ShopSettings.FirstOrDefault();

        var serviceSettings = TestRepository.CreateShopSettingsServicesTestData(Shops[0].Id, savedShopSettings.Id);
        serviceSettings.ForEach(s => dbContext.ShopSettings.Add(s.To<ShopSettings>()));
        dbContext.SaveChanges();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            services.SetSignalRHubTestSender(_signalRApplicationFactory.Server, ["events"]);
            
            FixtureLoggingContext.ConfigureServices(services);

            ConfigureContextServices?.Invoke(context, services);
        });
    }

}
