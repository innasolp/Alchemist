using Alchemist.Product.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Mapster;
using Alchemist.Test.ShopApiFactory;
using Test.PostresqlTestContainer;
using Microsoft.AspNetCore.Hosting;

namespace Alchemist.Product.Import.DBService.Test;

internal class ShopAPIWebAppFactory(string database, int httpPort, int httpsPort, TestServer signalRServer)
    : ShopApiConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbHelper>("ConnectionStrings:DbContext2",
            database, 5432, "postgres", "P@ssw0rd", httpPort, httpsPort, signalRServer)
{
    public event Action<IServiceCollection>? Configure;

    protected override void FillTestData(AlchemyContext dbContext)
    {
        var shops = TestRepository.GetShopsTestData(4);
        shops.ForEach(s => dbContext.Shops.Add(s.Adapt<Data.Shop>()));
        dbContext.SaveChanges();
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        Configure?.Invoke(services);
    }
}