using Alchemist.Product.Interfaces;

namespace Alchemist.Product.DbItemHandler;

internal interface IShopCachedRepository 
{
    Task<IShop?> TryGetShopAsync(string shopName, string shopUrl, CancellationToken cancellationToken = default);
}
