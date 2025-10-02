namespace Alchemist.Product.ShopWebApp.Models;

internal class ShopTabModel
{
    public IEnumerable<ShopItemModel> ShopItems { get; set; } = [];

    public ShopModel? CurrentShopModel { get; set; }
}
