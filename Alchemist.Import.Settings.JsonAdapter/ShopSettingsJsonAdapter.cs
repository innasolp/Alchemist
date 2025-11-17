using Alchemist.Import.Settings.Interfaces;
using Json.FileExtensions;

namespace Alchemist.Import.Settings.JsonAdapter;

public class ShopSettingsJsonAdapter<TShopImportSettings> : ISettingsAdapter
    where TShopImportSettings : class, IShopImportSettings
{
    private readonly string _jsonFilePath;

    public ShopSettingsJsonAdapter(string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;
    }

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName)
    {
        var shopImportSettings = (await _jsonFilePath.ReadFromJsonFileAsync<Dictionary<string,TShopImportSettings>>())
            ?.FirstOrDefault(s => s.Key == shopSettingsName).Value;

        if (shopImportSettings != null)
            return await Task.FromResult(shopImportSettings);

        return null;
    }

    public async Task<Dictionary<string, TShopImportSettings>> GetAllShopImportSettings()
    {
        return await _jsonFilePath.ReadFromJsonFileAsync<Dictionary<string, TShopImportSettings>>();
    }

    async Task<Dictionary<string, IShopImportSettings>> ISettingsAdapter.GetAllShopImportSettings()
    {
        var allShopImportSettings = await GetAllShopImportSettings();
        return allShopImportSettings.ToDictionary(s => s.Key, s => s.Value as IShopImportSettings);
    }
}