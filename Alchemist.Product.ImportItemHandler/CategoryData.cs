using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportItemHandler;

internal class CategoryData : ICategoryData
{
    public IShopCategory ShopCategory { get; set; }

    public IShopCategory ParentCategory { get; set; }

    public string ShopName { get; set; }

    public string ShopUrl { get; set; }
}
