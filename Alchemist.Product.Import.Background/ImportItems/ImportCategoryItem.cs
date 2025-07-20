using Alchemist.Product.ImportItem.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background.ImportItems;

internal class ImportCategoryItem : IImportCategoryItem
{
    public IShopCategory ShopCategory { get; set; }

    public IShopCategory ParentCategory { get; set; }

    public int ShopId { get; set; }
}
