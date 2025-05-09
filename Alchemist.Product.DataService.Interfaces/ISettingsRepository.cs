using Alchemist.Product.Interfaces;

namespace Alchemist.DataService.Interfaces;

public interface ISettingsRepository
{
    Task<IShopSettings?> GetShopSettings(int shopId, ShopSettingType settingType);

    Task<IShopSettings?> GetShopSettings(int id);

    Task<IShopSettings?> GetShopSettings(string shopSettingsName);

    Task<IShopSettings> SaveShopSettings(IShopSettings shopSettings);

    Task<bool> UpdateShopSettings(IShopSettings shopSettings);

    Task<List<IShopSettings>> GetChildSettings(int parentSettingsId);

    Task<List<IShopSettings>> SaveShopSettings(IShopSettings parentShopSettings, IEnumerable<IShopSettings> childrenSettings);

    Task<List<IShopSettings>> GetAllParentShopSettings();
}
