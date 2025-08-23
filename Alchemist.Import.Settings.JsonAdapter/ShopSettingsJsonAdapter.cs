using Alchemist.Import.Settings.Interfaces;
using Json.FileExtensions;
using Microsoft.Extensions.Logging;


namespace Alchemist.Import.Settings.JsonAdapter;

public class ShopSettingsJsonAdapter<TShopImportSettings> : ISettingsAdapter
    where TShopImportSettings : class, IShopImportSettings
{
    private readonly string _jsonFilePath;
    private readonly ILogger<ShopSettingsJsonAdapter<TShopImportSettings>> _logger;

    internal ShopSettingsJsonAdapter(ILogger<ShopSettingsJsonAdapter<TShopImportSettings>> logger, string jsonFilePath)
    {
        _jsonFilePath = jsonFilePath;
        _logger = logger;        
    }

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName)
    {

        var shopImportSettings = (await _jsonFilePath.ReadFromJsonFileAsync<TShopImportSettings[]>())
            ?.FirstOrDefault(s => s.Name == shopSettingsName);

        if (shopImportSettings != null)
            return await Task.FromResult(shopImportSettings);

        return null;
    }

    public async Task<List<IShopImportSettings>> GetAllShopImportSettings()
    {
        return (await _jsonFilePath.ReadFromJsonFileAsync<TShopImportSettings[]>())?.OfType<IShopImportSettings>().ToList();
    }
}