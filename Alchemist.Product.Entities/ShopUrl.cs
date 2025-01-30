using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Entities;

public  class ShopUrl:IShopUrl
{
    public int ShopId { get; set; }

    public string CategoryUrl { get; set; }

    public string ProductUrl { get; set; }

    public int? PageProductCount { get; set; }
}
