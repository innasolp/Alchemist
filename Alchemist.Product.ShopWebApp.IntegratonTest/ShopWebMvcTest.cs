using Alchemist.Product.ShopWebApp.IntegratonTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Xunit.Abstractions;

namespace Alchemist.Product.ShopWebApp.IntegratonTest;

public class ShopWebAppMvcFactory : ShopApiConfigurationLoggedWebAppFactory
{
    public ShopWebAppMvcFactory() : base(false, 8404, 8405, 8062, 8063,
        "ConnectionStrings:DbContext2",
        Common.ConfigurationHelper.GetSectionValue("ShopMvcTestDb"))
    {
    }
}

public class ShopWebMvcTest(ShopWebAppMvcFactory webAppFactory, ITestOutputHelper outputHelper)
    : LoggedContextTestFixture<ShopWebAppMvcFactory, ShopWebAppProgram>(webAppFactory, outputHelper)
{
    [Fact]
    public async Task IndexPageSuccessAsync()
    {
        var url = "/";

        try
        {
            var httpClient = WebAppFactory.CreateClient();
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                Assert.Contains("id=\"shopsTab\"", content);
            }
            catch
            {
                OutputHelper.WriteLine(content);
                throw;
            }
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }
}