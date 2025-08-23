namespace Alchemist.Import.Settings.Interfaces;

public interface ISettingsAdapter
{
    Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName);

    Task<List<IShopImportSettings>> GetAllShopImportSettings();
}
