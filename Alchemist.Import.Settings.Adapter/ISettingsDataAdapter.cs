using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.DataAdapter;

public interface ISettingsDataAdapter: ISettingsAdapter
{
    Task<IShopImportSettings?> GetShopImportSettings(int shopId);
        
    Task<IShopImportSettings> Save(IShopImportSettings shopSettingsModel);    
}
