using Microsoft.Extensions.DependencyInjection;
using Shop.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Shop.API.Client;

public class ShopApiClient : IShopDataService
{
    private readonly HttpClient _httpClient;

    public ShopApiClient([FromKeyedServices("ShopApiHttpClient")] HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public ShopApiClient(IHttpClientFactory httpClientFactory, [FromKeyedServices(nameof(ShopApiClient))] string apiHost)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(apiHost);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<IShopCategory?> AddShopCategory(IShopCategory shopCategory, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/ShopCategory", shopCategory.To<ShopCategory>(), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopCategory>(cancellationToken: cancellationToken);
    }

    public async Task<bool?> CheckCategoryForItemAncestor(int id, int ancestorItemId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/ShopCategory/checkancestoritem/{id}/{ancestorItemId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
    }

    public async Task<IShop> CreateShop(IShop shop, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Shop", shop.To<Shop>(), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>(cancellationToken: cancellationToken);
    }

    public async Task<List<IShopCategory>> GetAllCategoryChildren(int parentId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/ShopCategory/shopCategories/getAllChildren/{parentId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return [];
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ShopCategory>>(cancellationToken: cancellationToken);
        return [.. (result ?? []).OfType<IShopCategory>()];
    }

    public async Task<IShop?> GetShop(int id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Shop/{id}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>(cancellationToken: cancellationToken);
    }

    public async Task<IShop?> GetShopByName(string name, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Shop/byName?name={Uri.EscapeDataString(name)}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>(cancellationToken: cancellationToken);
    }

    public async Task<IShop?> GetShopByUrl(string url, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Shop/byUrl?url={Uri.EscapeDataString(url)}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>(cancellationToken: cancellationToken);
    }

    public async Task<List<IShopCategory>> GetShopCategories(int shopId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/ShopCategory/shopCategories/{shopId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return [];
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ShopCategory>>(cancellationToken: cancellationToken);
        return [.. (result ?? []).OfType<IShopCategory>()];
    }

    public async Task<IShopCategory?> GetShopCategoryByShopIdAndItemId(int shopId, int itemId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/ShopCategory/shopCategories/byShopIdAndItemId/{shopId}/{itemId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopCategory>(cancellationToken: cancellationToken);
    }

    public async Task<List<IShop>> GetShops(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Shop/Shops", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return [];
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Shop>>(cancellationToken: cancellationToken);
        return [.. (result ?? []).OfType<IShop>()];
    }

    public async Task<IShop> UpdateShop(IShop shop, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/Shop/Update", shop.To<Shop>(), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Shop>(cancellationToken: cancellationToken);
    }
}