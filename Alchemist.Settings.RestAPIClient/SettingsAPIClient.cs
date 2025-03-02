using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Alchemist.Settings.RestAPIClient;

public class SettingsAPIClient : IShopSettingsDataService
{
    private readonly HttpClient _httpClient;

    public SettingsAPIClient(IHttpClientFactory httpClientFactory, [FromKeyedServices(nameof(SettingsAPIClient))] string apiHost)
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
        var response = await _httpClient.GetAsync($"api/Settings/shopSettings/byShopId/{shopId}/{settingType}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(default(ShopSettings));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopSettings?>();
    }

    public async Task<IShopSettings?> GetShopSettings(int id)
    {
        var response = await _httpClient.GetAsync($"api/Settings/shopSettings/byId/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(default(ShopSettings));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopSettings?>();
    }

    public async Task<IShopSettings?> SaveShopSettings(IShopSettings shopSettings)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/Settings/shopSettings", shopSettings.To<ShopSettings>());
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopSettings>();
    }

    public async Task<List<IShopSettings>> SaveShopSettings(IShopSettings parentShopSettings, IEnumerable<IShopSettings> childrenSettings)
    {
        var shopSettingsWithServices = new ArrayList() { 
            parentShopSettings.To<ShopSettings>(), 
            childrenSettings.Select(s => s.To<ShopSettings>()).ToArray() };
        
        var response = await _httpClient.PostAsJsonAsync($"api/Settings/shopSettings/save", shopSettingsWithServices);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ArrayList>();
        if (result != null && result.Count >= 2)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var resultShopSettings = JsonSerializer.Deserialize<ShopSettings>(result[0].ToString(), options);
            var data = new List<IShopSettings> { resultShopSettings };

            var resultServices = JsonSerializer.Deserialize<ShopSettings[]>(result[1].ToString(), options);
            data.AddRange(resultServices);

            return await Task.FromResult(data);
        }

        return await Task.FromResult(default(List<IShopSettings>));
    }

    public async Task<bool> UpdateShopSettings(IShopSettings shopSettings)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Settings/shopSettings/update", shopSettings.To<ShopSettings>());
        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<List<IShopSettings>> GetChildSettings(int parentSettingsId)
    {
        var response = await _httpClient.GetAsync($"api/Settings/shopSettings/childSettings/{parentSettingsId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<ShopSettings>>())?.OfType<IShopSettings>().ToList() ?? [];
    }
}
