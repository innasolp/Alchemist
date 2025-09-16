using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Alchemist.Product.RestAPIClient;

public class ShopApiClient : IShopDataService
{
    private readonly HttpClient _httpClient;    

    public ShopApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public ShopApiClient(IHttpClientFactory httpClientFactory, [FromKeyedServices(nameof(ShopApiClient))] string apiHost)
        : this(httpClientFactory.CreateClient(apiHost)) { }    

    public async Task<IShopCategory?> AddShopCategory(IShopCategory shopCategory)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/ShopCategory", shopCategory.To<ShopCategory>());
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopCategory>();
    }

    public async Task<IShop> CreateShop(IShop shop)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Shop", shop.To<Shop>());
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>();
    }

    public async Task<List<IShopCategory>> GetAllCategoryChildren(int parentId)
    {
        var response = await _httpClient.GetAsync($"api/ShopCategory/shopCategories/getAllChildren/{parentId}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(new List<IShopCategory>());
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ShopCategory>>();
        return await Task.FromResult(result.OfType<IShopCategory>().ToList());
    }

    public async Task<IShop?> GetShop(int id)
    {
        var response = await _httpClient.GetAsync($"api/Shop/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(default(Shop));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>();
    }

    public async Task<IShop?> GetShopByName(string name)
    {
        var response = await _httpClient.GetAsync($"api/Shop/byName?name={name}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(default(Shop));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>();
    }

    public async Task<IShop?> GetShopByUrl(string url)
    {
        var response = await _httpClient.GetAsync($"api/Shop/byUrl/url={url}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(default(Shop));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>();
    }

    public async Task<List<IShopCategory>> GetShopCategories(int shopId)
    {
        var response = await _httpClient.GetAsync($"api/ShopCategory/shopCategories/{shopId}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(new List<IShopCategory>());
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ShopCategory>>();
        return await Task.FromResult(result.OfType<IShopCategory>().ToList());
    }

    public async Task<IShopCategory?> GetShopCategoryByShopIdAndItemId(int shopId, int itemId)
    {
        var response = await _httpClient.GetAsync($"api/ShopCategory/shopCategories/byShopIdAndItemId/{shopId}/{itemId}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(default(ShopCategory));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopCategory>();
    }

    public async Task<List<IShop>> GetShops()
    {
        var response = await _httpClient.GetAsync($"api/Shop/Shops");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(new List<IShop>());
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Shop>>();
        return await Task.FromResult(result.OfType<IShop>().ToList());
    }

    public async Task<IShop> UpdateShop(IShop shop)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/Shop/Update", shop.To<Shop>());
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>();
    }
}
