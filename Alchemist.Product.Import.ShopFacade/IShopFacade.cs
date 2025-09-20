namespace Alchemist.Product.Import.Model.Shop;

public interface IShopFacade
{
    bool TryGetShop(Guid guid, out IShopModel shop);

    Task<List<IShopModel>> UploadShops();

    IShopModel AddShop(Interfaces.IShop shop);

    List<IShopModel> GetShops();

    IShopModel CreateDefaultShop();

    Task SaveShop(IShopModel shop);
}
