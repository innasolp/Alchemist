using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory;
using Alchemist.Test.Server.Fixtures;
using Xunit.Abstractions;

namespace Shop.API.Test.Infrastructure;

public class ShopAPITestFixture<TWebAppFactory>(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : TestFixture<TWebAppFactory, ShopAPIProgram>(webAppFactory, outputHelper)
    where TWebAppFactory: DbContextWebAppFactory<ShopAPIProgram, AlchemyContext>
{
}
