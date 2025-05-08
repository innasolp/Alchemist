using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Adapter;

public interface ISettingsDataAdapter
{
    Task<IShopImportSettings?> GetShopImportSettings(int shopId, ShopSettingType shopSettingType);

    Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName);

    Task Save(IShopImportSettings shopSettingsModel);
}
