namespace Alchemist.Import.Settings;

public interface ISettingsAdapter
{
    Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName, CancellationToken cancellationToken = default);

    Task<Dictionary<string, IShopImportSettings>> GetAllShopImportSettings(CancellationToken cancellationToken = default);
}