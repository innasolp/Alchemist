using Shop.Interfaces;

namespace Shop.Import.Common;

public interface IShopCachedRepository 
{
    Task<IShop?> TryGetShopAsync(string shopName, string shopUrl, CancellationToken cancellationToken = default);
}