using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;

namespace Alchemist.Settings.RestAPI.Test;

public class SettingsApiConfigurationPostgresWebAppFactory
    : TestWebAppKestrelFactory<SettingsAPIProgram>, ILoggedContext, IAsyncLifetime
{
    private readonly SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> _signalRApplicationFactory;

    private readonly DbConfigurationContainerWebAppInterceptor<AlchemyContext, PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbChecker> _dbInterceptor;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public event Action<WebHostBuilderContext, IServiceCollection>? ConfigureContextServices;

    public Shop[] Shops { get; } = new Shop[2];

    public TestServer SignalRTestServer => _signalRApplicationFactory.Server;

    public SettingsApiConfigurationPostgresWebAppFactory() : base(8202, 8203)
    {
        _signalRApplicationFactory = new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>();
        _signalRApplicationFactory.CreateClient();

        _dbInterceptor = new DbConfigurationContainerWebAppInterceptor<AlchemyContext, PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbChecker>(this,
            "ConnectionStrings:DbContext2", "test_db_settings", "postgres", "P@ssw0rd", 5432, fillTestData : FillTestData);
    }

    protected void FillTestData(AlchemyContext dbContext)
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
        return _dbInterceptor.InitializeAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _dbInterceptor.DisposeAsync();
    }

    public Task ResetDatabaseIfAvailableAsync()
    {
        return _dbInterceptor.ResetDatabaseIfAvailableAsync();
    }
}