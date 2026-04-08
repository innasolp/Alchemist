using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Settings.RestAPI.Test;

internal class SettingsDbInterceptor(SettingsApiConfigurationPostgresWebAppFactory webHostConfigure) 
    : DbConfigurationContainerWebAppInterceptor<AlchemyContext, PostgresqlTestDbContainer, PostgresDbRespawner>(webHostConfigure,
        "ConnectionStrings:DbContext2", "test_db_settings", "postgres", "P@ssw0rd", 5432)
{
    public Shop[] Shops { get; } = new Shop[2];

    protected override void FillTestData(AlchemyContext dbContext)
    {
        var cnstr = dbContext.Database.GetConnectionString();

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
}

public class SettingsApiConfigurationPostgresWebAppFactory
    : TestWebAppKestrelFactory<SettingsAPIProgram>, ILoggedContext, IAsyncLifetime
{
    private readonly SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> _signalRApplicationFactory;

    private readonly SettingsDbInterceptor _dbInterceptor;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();
    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    public event Action<WebHostBuilderContext, IServiceCollection>? ConfigureContextServices;

    public Shop[] Shops => _dbInterceptor.Shops;

    public TestServer SignalRTestServer => _signalRApplicationFactory.Server;

    public SettingsApiConfigurationPostgresWebAppFactory() : base(8202, 8203)
    {
        _signalRApplicationFactory = new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>();
        _signalRApplicationFactory.CreateClient();

        _dbInterceptor = new SettingsDbInterceptor(this);
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

    public Task ResetDatabaseAsync()
    {
        return _dbInterceptor.ResetDatabaseAsync();
    }
}