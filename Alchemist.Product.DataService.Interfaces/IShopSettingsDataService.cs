using Alchemist.Product.Interfaces;

namespace Alchemist.DataService.Interfaces;

public interface IShopSettingsDataService
{
    Task<IShopSettings?> GetShopSettings(int shopId, ShopSettingType settingType);

    Task<IShopSettings?> GetShopSettings(int id);

    Task<IShopSettings?> AddShopSettings(IShopSettings shopSettings);

    Task<bool> UpdateShopSettings(IShopSettings shopSettings);

    Task<List<IShopSettings>> GetChildSettings(int parentSettingsId);
}
