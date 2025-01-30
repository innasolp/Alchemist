using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public class ShopProduct:IShopProduct
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public string ItemId { get; set; }

    public int ShopId { get; set; }

    public DateTime LastUpdate { get; set; }

    public bool? IsActual { get; set; }

    public string ApiUrl { get; set; }

    public string ItemUrl { get ; set; }
}
