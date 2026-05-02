using Shop.Interfaces;

namespace Alchemist.Product.CategoryData;

public interface ICategoryData
{
    IShopCategory ShopCategory { get; }

    IShopCategory? ParentCategory { get; }

    string ShopName { get; }

    string ShopUrl { get; }
}