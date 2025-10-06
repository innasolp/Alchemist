using Alchemist.Product.ShopWebApp.IntegratonTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
using Xunit.Abstractions;

namespace Alchemist.Product.ShopWebApp.IntegratonTest;

public class ShopWebAppApiFactory : ShopWebAppFactory
{
    public ShopWebAppApiFactory() : base (true, "ShopWebApiTestDb", 8402,8403,8060,8061)
    {
    }
}

public class ShopWebApiTest(ShopWebAppApiFactory webAppFactory, ITestOutputHelper outputHelper)
    : TestFixture<ShopWebAppApiFactory, ShopWebAppProgram>(webAppFactory, outputHelper)
{
    [Fact]
    public async Task SwaggerGenerationSuccessAsync()
    {
        var swaggerUrl = "/swagger/index.html";
        var httpClient = WebAppFactory.CreateClient();
        var response = await httpClient.GetAsync(swaggerUrl);       
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task HelloResponseSuccessAsync()
    {
        var httpClient = WebAppFactory.CreateClient();
        var response = await httpClient.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ShopWebApp API!", hello);
    }

    [Fact]
    public async Task GetViewContentSuccessAsync()
    {
        var httpClient = WebAppFactory.CreateClient();

        var url = $"/ShopApi/ShopList?hrefFormat={HttpUtility.UrlEncode("/Shop/{0}")}";
        var response = await httpClient.PostAsync(url, null);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("shop_item", content);
    }
}