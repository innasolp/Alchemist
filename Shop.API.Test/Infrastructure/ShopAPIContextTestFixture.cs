using Alchemist.Test.Server.Fixtures;
using Xunit.Abstractions;

namespace Shop.API.Test.Infrastructure;

public class ShopAPIConfigurationTestFixture<TWebAppFactory>(TWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
    : TestFixture<TWebAppFactory, ShopAPIProgram>(webAppFactory, outputHelper)
    where TWebAppFactory: TestHostServerWebAppFactory<ShopAPIProgram>
{
}