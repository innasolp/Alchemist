using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.Interfaces;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Import.WebApp.IntegrationTest.Infrastructure;

public class ShopAPIWebAppFactory(TestServer signalRServer) : DbAPIWebAppFactory<ShopAPIProgram, AlchemyContext>(true)
{
    private readonly TestServer _signalRServer = signalRServer;

    protected override void FillTestData(AlchemyContext dbContext)
    {
        var shops = TestRepository.GetShopsTestData(4);
        shops.ForEach(s => dbContext.Shops.Add(s.To<Shop>()));
        dbContext.SaveChanges();
    }

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder =>
        optionsBuilder.UseNpgsql("Host=localhost;Database=test_ci_db_importwebapp;Username=postgres;Password=P@ssw0rd;"));
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.SetSignalRTestSender(_signalRServer, ["events"]);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            context.SetKestrelLocalhostPortsConfig(8050, 8051);
        });
    }
}

