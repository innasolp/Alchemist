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

    public SettingsAPIClient([FromKeyedServices("SettingsApiHttpClient")] HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public SettingsAPIClient(IHttpClientFactory httpClientFactory, [FromKeyedServices(nameof(SettingsAPIClient))] string apiHost)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(apiHost);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<IShopSettings?> GetShopSettings(int shopId, ShopSettingType settingType, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Settings/byShopId/{shopId}/{(int)settingType}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopSettings?>(cancellationToken: cancellationToken);
    }

    public async Task<IShopSettings?> GetShopSettings(int id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Settings/byId/{id}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopSettings?>(cancellationToken: cancellationToken);
    }

    public async Task<IShopSettings?> SaveShopSettings(IShopSettings shopSettings, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/Settings", shopSettings.To<ShopSettings>(), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopSettings>(cancellationToken: cancellationToken);
    }

    public async Task<List<IShopSettings>> SaveShopSettings(IShopSettings parentShopSettings, IEnumerable<IShopSettings> childrenSettings, CancellationToken cancellationToken = default)
    {
       var shopSettingsWithServices = new {ShopSettings= parentShopSettings, Services = childrenSettings.Select(s => s.To<ShopSettings>()).ToArray() };

        var response = await _httpClient.PostAsJsonAsync($"api/Settings/save", shopSettingsWithServices, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ArrayList>(cancellationToken: cancellationToken);
        if (result != null && result.Count >= 2)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var resultShopSettings = JsonSerializer.Deserialize<ShopSettings>(result[0].ToString(), options);
            var data = new List<IShopSettings>();
            if (resultShopSettings != null)
                data.Add(resultShopSettings);

            var resultServices = JsonSerializer.Deserialize<ShopSettings[]>(result[1].ToString(), options) ?? [];
            data.AddRange(resultServices);

            return data;
        }

        return [];
    }

    public async Task<bool> UpdateShopSettings(IShopSettings shopSettings, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Settings/update", shopSettings.To<ShopSettings>(), cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<IShopSettings>> GetChildSettings(int parentSettingsId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Settings/childSettings/{parentSettingsId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var list = await response.Content.ReadFromJsonAsync<List<ShopSettings>>(cancellationToken: cancellationToken);
        return [.. (list ?? []).OfType<IShopSettings>()];
    }

    public async Task<IShopSettings?> GetShopSettings(string shopSettingsName, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Settings/byName?name={Uri.EscapeDataString(shopSettingsName)}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ShopSettings?>(cancellationToken: cancellationToken);
    }

    public async Task<List<IShopSettings>> GetAllParentShopSettings(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Settings/allParents", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return [];
        response.EnsureSuccessStatusCode();
        var list = await response.Content.ReadFromJsonAsync<List<ShopSettings>>(cancellationToken: cancellationToken);
        return [.. (list ?? []).OfType<IShopSettings>()];
    }
}