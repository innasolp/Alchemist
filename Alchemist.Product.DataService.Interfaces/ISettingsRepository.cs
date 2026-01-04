using Alchemist.Product.Interfaces;

namespace Alchemist.DataService.Interfaces;

public interface ISettingsRepository
{
    Task<IShopSettings?> GetShopSettings(int shopId, ShopSettingType settingType, CancellationToken cancellationToken = default);

    Task<IShopSettings?> GetShopSettings(int id, CancellationToken cancellationToken = default);

    Task<IShopSettings?> GetShopSettings(string shopSettingsName, CancellationToken cancellationToken = default);

    Task<IShopSettings> SaveShopSettings(IShopSettings shopSettings, CancellationToken cancellationToken = default);

    Task<bool> UpdateShopSettings(IShopSettings shopSettings, CancellationToken cancellationToken = default);

    Task<List<IShopSettings>> GetChildSettings(int parentSettingsId, CancellationToken cancellationToken = default);

    Task<List<IShopSettings>> SaveShopSettings(IShopSettings parentShopSettings, IEnumerable<IShopSettings> childrenSettings, CancellationToken cancellationToken = default);

    Task<List<IShopSettings>> GetAllParentShopSettings(CancellationToken cancellationToken = default);
}