using Alchemist.Product.Data;

namespace ShopSettings.UnitOfWork;

public interface IShopSettingsRepository
{
    Task<Alchemist.Product.Data.ShopSettings?> GetShopSettingsByShopId(int shopId, ShopSettingType settingType, CancellationToken cancellationToken = default);

    Task<Alchemist.Product.Data.ShopSettings?> SaveShopSettings(Alchemist.Product.Data.ShopSettings shopSettings, CancellationToken cancellationToken = default);

    Task<List<Alchemist.Product.Data.ShopSettings>> GetChildSettings(int parentSettingsId, CancellationToken cancellationToken = default);

    Task<List<Alchemist.Product.Data.ShopSettings>> SaveShopSettingsWithChildren(Alchemist.Product.Data.ShopSettings parentShopSettings, 
        IEnumerable<Alchemist.Product.Data.ShopSettings> childrenSettings, CancellationToken cancellationToken = default);

    Task<List<Alchemist.Product.Data.ShopSettings>> GetAllParentShopSettings(CancellationToken cancellationToken = default);
}
