using Alchemist.Product.Data;
using Alchemist.Test.Server.Fixtures;
using Xunit.Abstractions;

namespace Alchemist.Product.RestAPI.Test;

public class ShopAPITestFixture<TWebAppFactory>(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : TestFixture<TWebAppFactory, Startup>(webAppFactory, outputHelper)
    where TWebAppFactory: AlchemistDbContextWebAppFactory<Startup, AlchemyContext>
{
}
