using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.DataAdapter;

public interface ISettingsDataAdapter: ISettingsAdapter
{
    Task<IShopImportSettings?> GetShopImportSettings(int shopId, ShopSettingType shopSettingType);

    Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName);

    Task Save(IShopImportSettings shopSettingsModel);    
}
