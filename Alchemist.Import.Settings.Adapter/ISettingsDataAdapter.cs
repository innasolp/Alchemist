using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Adapter;

public interface ISettingsDataAdapter
{
    Task<IShopImportSettings?> GetShopSettings(int shopId, ShopSettingType shopSettingType);

    Task Save(IShopImportSettings shopSettingsModel);
}
