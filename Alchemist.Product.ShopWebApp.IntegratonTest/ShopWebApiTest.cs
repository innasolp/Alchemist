using Alchemist.Product.ShopWebApp.Controllers;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.ShopWebAppFactory;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.ShopWebApp.IntegratonTest;

public class ShopWebAppApiFactory : ShopWebAppFullFactory
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

        var data = new ShopApiData { HRefFormat = "/Shop/{0}" };
        var url = $"/ShopApi/ShopList";
        var response = await httpClient.PostAsync(url, JsonContent.Create(data));

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("shop_item", content);
    }
}