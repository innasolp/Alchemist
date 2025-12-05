using Alchemist.Product.Interfaces;

namespace Alchemist.Product.DbItemHandler;

internal interface IShopCache 
{
    Task<IShop?> TryGetShopAsync(string shopName, string shopUrl);
}
