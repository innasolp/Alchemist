using Alchemist.Product.Entities;
using Shop.Interfaces;

namespace Alchemist.Product.CategoryData;

public class CategoryData : ICategoryData
{
    public ShopCategory ShopCategory { get; set; }

    public ShopCategory? ParentCategory { get; set; }

    public string ShopName { get; set; }

    public string ShopUrl { get; set; }

    IShopCategory ICategoryData.ShopCategory => ShopCategory;

    IShopCategory? ICategoryData.ParentCategory => ParentCategory;
}