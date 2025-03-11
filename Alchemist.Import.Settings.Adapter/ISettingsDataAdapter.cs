using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Adapter;

public interface ISettingsDataAdapter<TProductShopImportSettings, TCategoryShopImportSettings, TImportServiceSettings>
    where TProductShopImportSettings : class, IProductShopImportSettings
    where TCategoryShopImportSettings : class, ICategoryShopImportSettings
    where TImportServiceSettings : class, IImportServiceSettings
{
    Task<IShopImportSettings?> GetShopSettings(int shopId, ShopSettingType shopSettingType);

    Task Save(IShopImportSettings shopSettingsModel);
}
