namespace Alchemist.Import.Products.Interfaces;

public interface IShopProductModel : IProductItem
{
    int ShopId { get; set; }

    int CategoryItemId { get; set; }

    string ApiUrl { get; set; }
}
