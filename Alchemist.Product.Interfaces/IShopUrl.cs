namespace Alchemist.Product.Interfaces;

public interface IShopUrl
{
    public int ShopId { get; set; }

    public string CategoryUrl { get; set; }

    public string ProductUrl { get; set; }

    public int? PageProductCount { get; set; }
}
