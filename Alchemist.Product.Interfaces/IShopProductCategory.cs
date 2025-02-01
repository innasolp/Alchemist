namespace Alchemist.Product.Interfaces;

public interface IShopProductCategory
{
    long Id { get; set; }

    long ShopProductId {  get; set; }

    int ShopCategoryId {  get; set; }
}
