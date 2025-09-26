namespace Alchemist.Product.ShopWebApp.Models;

internal class ShopTabModel
{
    public IEnumerable<ShopItemModel> ShopItems { get; set; } = new List<ShopItemModel>();

    public ShopModel? CurrentShopModel { get; set; }

    public int? CurrentShopId { get; set; }
}
