using Alchemist.Product.Data;
using Alchemist.Test.Server.Fixtures;
using Xunit.Abstractions;

namespace Alchemist.Product.RestAPI.Test;

public class ShopAPITestFixture<TWebAppFactory>(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : TestFixture<TWebAppFactory, ShopAPIProgram>(webAppFactory, outputHelper)
    where TWebAppFactory: DbContextWebAppFactory<ShopAPIProgram, AlchemyContext>
{
}
