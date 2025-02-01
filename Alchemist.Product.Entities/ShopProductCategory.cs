using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class ShopProductCategory : IShopProductCategory
{
    public long Id { get; set; }

    public long ShopProductId { get; set; }

    public int ShopCategoryId { get; set; }
}
