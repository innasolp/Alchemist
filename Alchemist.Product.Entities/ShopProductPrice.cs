using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class ShopProductPrice:IShopProductPrice
{
    public long Id { get; set; }

    public long ShopProductId { get; set; }

    public double Price { get; set; }

    public int CurrencyId { get; set; }
}
