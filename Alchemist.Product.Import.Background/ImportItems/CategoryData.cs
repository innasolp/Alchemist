using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background.ImportItems;

internal class CategoryData : ICategoryData
{
    public IShopCategory ShopCategory { get; set; }

    public IShopCategory ParentCategory { get; set; }

    public int ShopId { get; set; }
}
