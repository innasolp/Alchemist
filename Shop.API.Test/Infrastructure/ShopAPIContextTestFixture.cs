using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.DBApiWebAppFactory.Context;
using Alchemist.Test.Server.Fixtures;
using Xunit.Abstractions;

namespace Shop.API.Test.Infrastructure;

public class ShopAPIContextTestFixture<TWebAppFactory>(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : TestFixture<TWebAppFactory, ShopAPIProgram>(webAppFactory, outputHelper)
    where TWebAppFactory: DbContextWebAppFactory<ShopAPIProgram, AlchemyContext>
{
}
public class ShopAPIConfigurationTestFixture<TWebAppFactory>(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : TestFixture<TWebAppFactory, ShopAPIProgram>(webAppFactory, outputHelper)
    where TWebAppFactory: DbConfigurationWebAppFactory<ShopAPIProgram, AlchemyContext>
{
}