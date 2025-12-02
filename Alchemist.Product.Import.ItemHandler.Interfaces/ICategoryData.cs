using Alchemist.Product.Interfaces;

namespace Alchemist.Product.DataItem.Interfaces;

public interface ICategoryData
{
    IShopCategory ShopCategory { get; }

    IShopCategory ParentCategory { get; }

    string ShopName { get; }

    string ShopUrl { get; }
}
