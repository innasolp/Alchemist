using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Alchemist.Settings.RestAPIClient;

public class SettingsAPIClient:IShopSettingsDataService
{
    private readonly HttpClient _httpClient;

    public SettingsAPIClient(IHttpClientFactory httpClientFactory, string apiHost)
    {
        // _httpClient = httpClientFactory.CreateClient();
        _httpClient = httpClientFactory.CreateClient(apiHost);
        _httpClient.BaseAddress = new Uri(apiHost);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<IShopSettings?> GetShopSettings(int shopId, ShopSettingType settingType)
    {
        var response = await _httpClient.GetAsync($"api/Shop/shopSettings/byShopId/{shopId}/{settingType}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(default(ShopSettings));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopSettings?>();
    }
    
    public async Task<IShopSettings?> GetShopSettings(int id)
    {
        var response = await _httpClient.GetAsync($"api/Shop/shopSettings/byId/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(default(ShopSettings));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopSettings?>();
    }

    public async Task<IShopSettings?> AddShopSettings(IShopSettings shopSettings)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/Shop/shopSettings", shopSettings.To<ShopSettings>());
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopSettings>();
    }

    public async Task<bool> UpdateShopSettings(IShopSettings shopSettings)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Shop/shopSettings/update", shopSettings.To<ShopSettings>());
        response.EnsureSuccessStatusCode();
        return true;
    }
}
