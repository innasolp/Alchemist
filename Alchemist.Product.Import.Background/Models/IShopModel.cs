using Alchemist.Import.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background.Models;

internal interface IShopModel : IShopItem, IShop
{
    new string ShopName { get; set; }

    new string ShopUrl { get; set; }

    new string Host { get; set; }
}
