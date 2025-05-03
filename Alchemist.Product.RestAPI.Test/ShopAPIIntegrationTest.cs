using Alchemist.Product.Data;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.RestAPI.Test;

public class ShopAPIIntegrationTest : ShopAPITestFixture<ShopAPISignlRMockWebAppFactory>
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
        
        var shop = await response.Content.ReadFromJsonAsync<Shop>();
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
        var shop = new Shop() { Name = "TestShopNew", Url = "https://testshopnew" };
        var response = await _httpClient.PutAsJsonAsync($"api/Shop", shop);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        var getNewResponse = await _httpClient.GetAsync($"api/Shop/byName?name={shop.Name}");
        var shopNew = await getNewResponse.Content.ReadFromJsonAsync<Shop>();
        Assert.NotNull(shopNew);
        Assert.Equal(shop.Name, shopNew.Name);
    }

    [Fact]
    public async Task ResponseInternalErrorOnCreateShopWithExistsIdAsync()
    {
        var shop = new Shop() { Name = "TestShopNew", Url = "https://testshopnew", Id = 1 };
        var response = await _httpClient.PutAsJsonAsync($"api/Shop", shop);

        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }
}