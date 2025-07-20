using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportItem.Interfaces;

public interface IImportCategoryItem
{
    IShopCategory ShopCategory { get; }

    IShopCategory ParentCategory { get; }

    int ShopId { get; }
}
