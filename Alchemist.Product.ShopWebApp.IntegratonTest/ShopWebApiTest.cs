using Alchemist.Product.ShopWebApp.Controllers;
using Alchemist.Product.ShopWebApp.IntegratonTest.Infrastructure;
using Alchemist.Test.Server.Fixtures;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.ShopWebApp.IntegratonTest;

public class ShopWebAppApiFactory : ShopApiConfigurationLoggedWebAppFactory
{
    public ShopWebAppApiFactory() : base(true, 8402, 8403, 8060, 8061,
        "ConnectionStrings:DbContext2",
        Common.ConfigurationHelper.GetSectionValue("ShopWebApiTestDb"))
    {
    }
}

public class ShopWebApiTest(ShopWebAppApiFactory webAppFactory, ITestOutputHelper outputHelper)
    : LoggedContextTestFixture<ShopWebAppApiFactory, ShopWebAppProgram>(webAppFactory, outputHelper)
{
    [Fact]
    public async Task SwaggerGenerationSuccessAsync()
    {
        var swaggerUrl = "/swagger/index.html";

        try
        {
            var httpClient = WebAppFactory.CreateClient();
            var response = await httpClient.GetAsync(swaggerUrl);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task HelloResponseSuccessAsync()
    {
        try
        {
            var httpClient = WebAppFactory.CreateClient();
            var response = await httpClient.GetAsync("/");
            response.EnsureSuccessStatusCode();
            var hello = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello ShopWebApp API!", hello);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task GetViewContentSuccessAsync()
    {
        var url = $"/ShopApi/ShopList";
        var data = new ShopApiData { HRefFormat = "/Shop/{0}" };

        try
        {
            var httpClient = WebAppFactory.CreateClient();

            var response = await httpClient.PostAsync(url, JsonContent.Create(data));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("shop_item", content);
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
    }

    [Fact]
    public async Task LoadShopTabSuccessAsync()
    {
        var url = "/ShopApi/Shop/1";

        try
        {
            var httpClient = WebAppFactory.CreateClient();
            var response = await httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                Assert.Contains("left-menu-ul shopsList", content);
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

    [Fact]
    public async Task NewShopSuccessAsync()
    {
        var url = "/ShopApi/Shop/New";
        try
        {
            var httpClient = WebAppFactory.CreateClient();
            var response = await httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                Assert.Contains("left-menu-ul shopsList", content);
                Assert.DoesNotContain("shop_item selected", content);
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