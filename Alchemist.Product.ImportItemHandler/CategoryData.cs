using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportItemHandler;

internal class CategoryData : ICategoryData
{
    public IShopCategory ShopCategory { get; set; }

    public IShopCategory ParentCategory { get; set; }

    public int ShopId { get; set; }
}
