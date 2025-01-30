using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Data;

public partial class ShopProductPrice
{
    public long Id { get; set; }

    public long ShopProductId { get; set; }

    public double Price { get; set; }

    public DateTime LastUpdate { get; set; }

    public int CurrencyId { get; set; }
}
