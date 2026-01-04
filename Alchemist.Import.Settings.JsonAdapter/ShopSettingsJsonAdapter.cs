using Alchemist.Common;

namespace Alchemist.Import.Settings.JsonAdapter;

public class ShopSettingsJsonAdapter<TShopImportSettings>(string jsonFilePath) : ISettingsAdapter
    where TShopImportSettings : class, IShopImportSettings
{
    private readonly string _jsonFilePath = jsonFilePath;

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName, CancellationToken cancellationToken = default)
    {
        var shopImportSettings = (await _jsonFilePath.ReadFromJsonFileAsync<Dictionary<string,TShopImportSettings>>(cancellationToken : cancellationToken))
            ?.FirstOrDefault(s => s.Key == shopSettingsName).Value;

        if (shopImportSettings != null)
            return await Task.FromResult(shopImportSettings);

        return null;
    }

    public async Task<Dictionary<string, TShopImportSettings>> GetAllShopImportSettings(CancellationToken cancellationToken = default)
    {
        return await _jsonFilePath.ReadFromJsonFileAsync<Dictionary<string, TShopImportSettings>>(cancellationToken: cancellationToken);
    }

    async Task<Dictionary<string, IShopImportSettings>> ISettingsAdapter.GetAllShopImportSettings(CancellationToken cancellationToken = default)
    {
        var allShopImportSettings = await GetAllShopImportSettings(cancellationToken);
        return allShopImportSettings.ToDictionary(s => s.Key, s => s.Value as IShopImportSettings);
    }
}