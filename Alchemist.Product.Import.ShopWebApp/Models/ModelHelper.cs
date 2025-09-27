namespace Alchemist.Product.ShopWebApp.Models;

internal static class ModelHelper
{
    public static ShopModel? GetSelectedShop(IEnumerable<ShopModel> shops, int? selectedShopId = null)
    {
        return selectedShopId != 0
            ? shops.FirstOrDefault(s => s.Id == selectedShopId) ?? shops.FirstOrDefault()
            : null;
    }

    public static IEnumerable<ShopItemModel> GetShopItemModels(IEnumerable<ShopModel> shops, 
        int? selectedShopId = null,
        string shopRefFormat = "/Shop/{0}")
    {        
        return shops.Select(s => new ShopItemModel
        {
            ShopModel = s,
            IsSelected = s.Id == selectedShopId,
            HRef = string.Format(shopRefFormat, s.Id)
        });
    }

    internal static ShopTabModel GetShopTabModel(IEnumerable<ShopModel> shops, int? selectedShopId = null, string shopRefFormat = "/Shop/{0}")
    {
        var selectedShop = GetSelectedShop(shops, selectedShopId);
        var shopItems = GetShopItemModels(shops, selectedShop?.Id, shopRefFormat);
        return new ShopTabModel
        {
            ShopItems = shopItems,
            CurrentShopModel = selectedShop
        };
    }
}
