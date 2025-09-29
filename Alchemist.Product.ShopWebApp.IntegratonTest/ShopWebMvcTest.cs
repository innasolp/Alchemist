using Alchemist.Product.ShopWebApp.IntegratonTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using Xunit.Abstractions;

namespace Alchemist.Product.ShopWebApp.IntegratonTest;

public class ShopWebAppMvcFactory : ShopWebAppFactory
{
    public ShopWebAppMvcFactory() : base(false, "ShopMvcTestDb", 8404, 8405, 8062, 8063)
    {
    }
}

public class ShopWebMvcTest(ShopWebAppMvcFactory webAppFactory, ITestOutputHelper outputHelper)
    : TestFixture<ShopWebAppMvcFactory, ShopWebAppProgram>(webAppFactory, outputHelper)
{
    [Fact]
    public async Task LoadShopTabSuccessAsync()
    {
        var url = "/Shop/ShopTab";
        var httpClient = WebAppFactory.CreateClient();
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            Assert.Contains("left-menu-ul shopsList", content);
            Assert.Contains("shop_item selected", content);
        }
        catch
        {
            OutputHelper.WriteLine(content);
            throw;
        }
    }

    [Fact]
    public async Task IndexPageSuccessAsync()
    {
        var url = "/";
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
}
