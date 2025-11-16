using Alchemist.DataService.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ShopWebApp.Models;

internal class ShopFacade(IShopDataService shopDataService)
{
    private readonly IShopDataService _shopDataService = shopDataService;

    public async Task<List<ShopModel>> GetShops()
    {
        var uploadedShops = await _shopDataService.GetShops();
        var result = new List<ShopModel>();
        uploadedShops.ForEach(s => result.Add(s.To<ShopModel>()));
        return result;
    }

    public async Task<ShopModel> GetShop(int shopId)
    {
        var shop = await _shopDataService.GetShop(shopId);
        return shop?.To<ShopModel>() ?? new ShopModel();
    }

    public async Task<ShopModel> SaveShop(IShop shop)
    {  
        var savedShop = shop.Id == 0
          ? await _shopDataService.CreateShop(shop) :
            await _shopDataService.UpdateShop(shop);

        return savedShop.To<ShopModel>();
    }
}
