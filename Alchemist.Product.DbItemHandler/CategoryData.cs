using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.DbItemHandler;

internal class CategoryData : ICategoryData
{
    public ShopCategory ShopCategory { get; set; }

    public ShopCategory ParentCategory { get; set; }

    public int ShopId { get; set; }

    IShopCategory ICategoryData.ShopCategory => ShopCategory;

    IShopCategory ICategoryData.ParentCategory => ParentCategory;
}
