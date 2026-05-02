namespace Alchemist.Import.Settings.DataAdapter;

public interface ISettingsDataAdapter: ISettingsAdapter
{
    Task<IShopImportSettings?> GetShopImportSettings(int shopId, CancellationToken cancellationToken = default);
        
    Task<IShopImportSettings> Save(IShopImportSettings shopSettingsModel, CancellationToken cancellationToken = default);    
}
