using Alchemist.DataService.Interfaces;
using Alchemist.Product.Interfaces;
using System.Collections.Concurrent;

namespace Alchemist.Product.DbItemHandler;

internal class ShopCache(IShopDataService shopDataService) : IShopCache
{
    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly ConcurrentDictionary<string, IShop> _shops = new();
    public async Task<IShop?> TryGetShopAsync(string shopName, string shopUrl)
    {
        if (!_shops.TryGetValue(shopName, out var shop) && !_shops.TryGetValue(shopUrl, out shop))
        {
            shop = await _shopDataService.GetShopByName(shopName);
            if (shop != null)
                _shops.TryAdd(shopName, shop);
            else
            {
                shop = await _shopDataService.GetShopByUrl(shopUrl);
                if (shop != null)
                    _shops.TryAdd(shopUrl, shop);
            }
        }

        return shop;
    }
}