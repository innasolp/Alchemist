using Alchemist.Product.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shop.API.Test.Infrastructure;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Shop.API.Test;

public class ShopAPISignlRMockWebAppFactory : ShopApiConfigurationWebAppFactory
{
    public ShopAPISignlRMockWebAppFactory() 
        : base("DbContext2", "alchemy", SignalRCommon.ConfigureSignalRMock)
    { 
    }

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {}
}

public class ShopAPIIntegrationTest : ShopAPIConfigurationTestFixture<ShopAPISignlRMockWebAppFactory>
{
    private readonly HttpClient _httpClient;

    public ShopAPIIntegrationTest(ShopAPISignlRMockWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : base(webAppFactory, outputHelper)
    {
        _httpClient = WebAppFactory.CreateClient();
    }

    [Fact]
    public async Task GetShopSuccessAsync()
    {
        var name = "TestShop";
        var response = await _httpClient.GetAsync($"api/Shop/byName?name={name}");
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        var shop = await response.Content.ReadFromJsonAsync<Alchemist.Product.Data.Shop>();
        Assert.NotNull(shop);
        Assert.Equal(name, shop.Name);
    }

    [Fact]
    public async Task ResponseStatusBadRequestOnGetShopWithEmptyNameAsync()
    {
        var response = await _httpClient.GetAsync($"api/Shop/byName?name={""}");
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResponseStatusNotFoundOnGetShopWithNotExistsNameAsync()
    {
        var response = await _httpClient.GetAsync($"api/Shop/byName?name={Guid.NewGuid().ToString()}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateShopSuccessAsync()
    {
        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew" };
        var response = await _httpClient.PutAsJsonAsync($"api/Shop", shop);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        var getNewResponse = await _httpClient.GetAsync($"api/Shop/byName?name={shop.Name}");
        getNewResponse.EnsureSuccessStatusCode();
        var shopNew = await getNewResponse.Content.ReadFromJsonAsync<Alchemist.Product.Data.Shop>();
        Assert.NotNull(shopNew);
        Assert.Equal(shop.Name, shopNew.Name);
    }

    [Fact]
    public async Task ResponseInternalErrorOnCreateShopWithExistsIdAsync()
    {
        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew", Id = 1 };
        var response = await _httpClient.PutAsJsonAsync($"api/Shop", shop);

        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }
}