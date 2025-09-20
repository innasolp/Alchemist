namespace Alchemist.Product.Import.Model.Shop;

public interface IShopModelFactory
{
    IShopModel CreateShopModel(int shopId);
}
