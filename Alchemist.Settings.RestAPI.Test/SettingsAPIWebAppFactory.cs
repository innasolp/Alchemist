using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.DBApiWebAppFactory;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;

namespace Alchemist.Settings.RestAPI.Test;

public class SettingsAPIWebAppFactory : DbContextWebAppFactory<SettingsAPIProgram, AlchemyContext>, ILoggedContext, IAsyncLifetime
{
    private readonly SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> _signalRApplicationFactory;

    private readonly PostgreSqlContainer _postgreSqlContainer;
    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();
    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public event Action<WebHostBuilderContext, IServiceCollection> ConfigureContextServices;

    public Shop[] Shops { get; } = new Shop[2];

    public TestServer SignalRTestServer => _signalRApplicationFactory.Server;

    public SettingsAPIWebAppFactory()
    {
        _postgreSqlContainer = PostresqlTestContainerHelper.BuildPostgreSqlContainer(Guid.NewGuid().ToString());

        _signalRApplicationFactory = new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>();
        _signalRApplicationFactory.CreateClient();
    }

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder =>
        optionsBuilder.UseNpgsql(_postgreSqlContainer.BuildConnectionString(DataBase)));
    }

    protected override void FillTestData(AlchemyContext dbContext)
    {
        Shops[0] = dbContext.Shops.Add(new Shop { Name = "TestShop1", Url = "https://testshop2" }).Entity;        
        Shops[1] = dbContext.Shops.Add(new Shop { Name = "TestShop1", Url = "https://testshop2" }).Entity;

        dbContext.SaveChanges();        

       var shopSettings = TestRepository.CreateCategoryShopSettings(Shops[0].Id, Shops[0].Name); 

        dbContext.ShopSettings.Add(shopSettings);
        dbContext.SaveChanges();
        var savedShopSettings = dbContext.ShopSettings.FirstOrDefault();

        var serviceSettings = TestRepository.CreateShopSettingsServicesTestData(Shops[0].Id, savedShopSettings.Id);
        serviceSettings.ForEach(s => dbContext.ShopSettings.Add(s));
        dbContext.SaveChanges();
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        services.SetSignalRHubTestSender(_signalRApplicationFactory.Server, ["events"]);

        FixtureLoggingContext.ConfigureServices(services);

        ConfigureContextServices?.Invoke(context, services);
    }

    public Task InitializeAsync()
    {
        return _postgreSqlContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}