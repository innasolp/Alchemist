namespace Alchemist.Import.Settings;

public interface ISettingsAdapter
{
    Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName);

    Task<Dictionary<string, IShopImportSettings>> GetAllShopImportSettings();
}