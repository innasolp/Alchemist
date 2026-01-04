using Alchemist.DataService.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ShopWebApp.Models;

internal class ShopFacade(IShopDataService shopDataService)
{
    private readonly IShopDataService _shopDataService = shopDataService;

    public async Task<List<ShopModel>> GetShops(CancellationToken cancellationToken = default)
    {
        var uploadedShops = await _shopDataService.GetShops(cancellationToken);
        var result = new List<ShopModel>();
        uploadedShops.ForEach(s => result.Add(s.To<ShopModel>()));
        return result;
    }

    public async Task<ShopModel> GetShop(int shopId, CancellationToken cancellationToken = default)
    {
        var shop = await _shopDataService.GetShop(shopId, cancellationToken);
        return shop?.To<ShopModel>() ?? new ShopModel();
    }

    public async Task<ShopModel> SaveShop(IShop shop, CancellationToken cancellationToken = default)
    {  
        var savedShop = shop.Id == 0
          ? await _shopDataService.CreateShop(shop, cancellationToken) :
            await _shopDataService.UpdateShop(shop, cancellationToken);

        return savedShop.To<ShopModel>();
    }
}