using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Alchemist.Product.Import.Backgorund.Service.IntegrationTest;
using Alchemist.Test.DBApiWebAppFactory;

namespace Alchemist.Product.Import.DBService.Test;

internal class ShopAPIWebAppFactory(string connectionString, TestServer signalRServer) : DbAPIWebAppFactory<ShopAPIProgram, AlchemyContext>(true)
{
    private readonly TestServer _signalRServer = signalRServer;

    private readonly string _connectionString = connectionString;

    protected override void FillTestData(AlchemyContext dbContext)
    {
        var shops = TestRepository.GetShopsTestData(4);
        shops.ForEach(s => dbContext.Shops.Add(s.To<Shop>()));
        dbContext.SaveChanges();
    }

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder => optionsBuilder.UseNpgsql(_connectionString));
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.SetSignalRHubTestSender(_signalRServer, ["events"]);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            context.Configuration.SetKestrelLocalhostPortsConfig(8050, 8051);
        });
    }
}

