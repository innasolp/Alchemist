using Alchemist.DataService.Interfaces;
using Alchemist.Product.Interfaces;
using System.Collections.Concurrent;

namespace Alchemist.Product.DbItemHandler;

internal class ShopCachedRepository(IShopDataService shopDataService) : IShopCachedRepository
{
    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly ConcurrentDictionary<string, IShop> _shops = new();
    public async Task<IShop?> TryGetShopAsync(string shopName, string shopUrl, CancellationToken cancellationToken = default)
    {
        if (!_shops.TryGetValue(shopName, out var shop) && !_shops.TryGetValue(shopUrl, out shop))
        {
            shop = await _shopDataService.GetShopByName(shopName, cancellationToken);
            if (shop != null)
                _shops.TryAdd(shopName, shop);
            else
            {
                shop = await _shopDataService.GetShopByUrl(shopUrl, cancellationToken);
                if (shop != null)
                    _shops.TryAdd(shopUrl, shop);
            }
        }

        return shop;
    }
}