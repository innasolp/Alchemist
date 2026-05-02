using Alchemist.Product.Entities;

namespace Alchemist.Product.CategoryData;

public class CategoryData
{
    public ShopCategory ShopCategory { get; init; }

    public ShopCategory? ParentCategory { get; init; }

    public string ShopName { get; init; }

    public string ShopUrl { get; init; }
}