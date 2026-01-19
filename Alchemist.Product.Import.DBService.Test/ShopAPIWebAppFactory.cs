using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Product.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Alchemist.Test.DBApiWebAppFactory;
using Mapster;

namespace Alchemist.Product.Import.DBService.Test;

internal class ShopAPIWebAppFactory(string connectionString, TestServer signalRServer) 
    : DBAPIKestrelWebAppFactory<ShopAPIProgram, AlchemyContext>(true, 8052, 8053)
{
    private readonly TestServer _signalRServer = signalRServer;

    private readonly string _connectionString = connectionString;

    public event Action<IServiceCollection> Configure;

    protected override void FillTestData(AlchemyContext dbContext)
    {
        var shops = TestRepository.GetShopsTestData(4);
        shops.ForEach(s => dbContext.Shops.Add(s.Adapt<Data.Shop>()));
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

        Configure?.Invoke(services);
    }
}

