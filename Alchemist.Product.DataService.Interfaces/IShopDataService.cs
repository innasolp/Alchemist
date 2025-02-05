using Alchemist.Product.Interfaces;

namespace Alchemist.DataService.Interfaces;

public interface IShopDataService
{
    Task<List<IShop>> GetShops();
    Task<IShop> CreateShop(IShop shop);
    Task<IShop?> GetShop(int id);
    Task<IShopUrl?> GetShopUrl(int shopId);

    Task<IShopUrl?> CreateShopUrl(IShopUrl shopUrl);

    Task<IShopCategory?> AddShopCategory(IShopCategory shopCategory);

    Task<List<IShopCategory>> GetShopCategories(int shopId);

    Task<IShop?> GetShopByName(string name);

    Task<IShop?> GetShopByUrl(string url);

    Task<IShopCategory?> GetShopCategoryByShopIdAndItemId(int shopId, int itemId);
}
