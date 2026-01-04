using Alchemist.Product.Interfaces;

namespace Alchemist.DataService.Interfaces;

public interface IShopDataService
{
    Task<List<IShop>> GetShops(CancellationToken cancellationToken = default);

    Task<IShop> CreateShop(IShop shop, CancellationToken cancellationToken = default);

    Task<IShop> UpdateShop(IShop shop, CancellationToken cancellationToken = default);

    Task<IShop?> GetShop(int id, CancellationToken cancellationToken = default);    

    Task<IShopCategory> AddShopCategory(IShopCategory shopCategory, CancellationToken cancellationToken = default);

    Task<List<IShopCategory>> GetShopCategories(int shopId, CancellationToken cancellationToken = default);

    Task<IShop?> GetShopByName(string name, CancellationToken cancellationToken = default);

    Task<IShop?> GetShopByUrl(string url, CancellationToken cancellationToken = default);

    Task<IShopCategory?> GetShopCategoryByShopIdAndItemId(int shopId, int itemId, CancellationToken cancellationToken = default);

    Task<List<IShopCategory>> GetAllCategoryChildren(int parentId, CancellationToken cancellationToken = default);
}