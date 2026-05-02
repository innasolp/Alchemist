namespace Alchemist.Product.Data;

public partial class ShopProductPrice
{
    public long Id { get; set; }

    public long ShopProductId { get; set; }

    public double Price { get; set; }

    public int CurrencyId { get; set; }

    public DateTime AddedTs { get; set; }

    public DateTime? UpdatedTs { get; set; }
}
