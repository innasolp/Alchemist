using Alchemist.Product.Data;
using Alchemist.Test.ShopApiFactory;
using Mapster;
using Microsoft.AspNetCore.TestHost;
using Test.PostresqlTestContainer;

namespace Alchemist.Product.Import.DBService.Test;

internal class ShopAPIWebAppFactory(string database, int httpPort, int httpsPort, TestServer signalRServer)
    : ShopApiConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner, PostgresDbHelper>("ConnectionStrings:DbContext2",
            database, 5432, "postgres", "P@ssw0rd", httpPort, httpsPort, signalRServer)
{
    protected override void FillTestData(AlchemyContext dbContext)
    {
        var shops = TestRepository.GetShopsTestData(4);
        shops.ForEach(s => dbContext.Shops.Add(s.Adapt<Data.Shop>()));
        dbContext.SaveChanges();
    }
}