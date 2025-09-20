using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ShopModelExtensions
{
    public static void Update(this IShopModel target, IShopModel shop)
    {
        target.Name = shop.Name;
        target.Url = shop.Url;
        target.Caption = shop.Caption;
        target.IsDeprecated = shop.IsDeprecated;
    }

    public static void SetFrom(this IShopModel target, IShop shop)
    {
        target.Name = shop.Name;
        target.Url = shop.Url;
        target.Caption = shop.Caption;
    }
}
