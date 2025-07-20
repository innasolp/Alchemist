using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.ImportItem.Interfaces;

namespace Alchemist.Product.Import.Background.ImportItems;

public static class ImportItemHelper
{
    public static IImportProductItem ConvertToImportProductItem(this IProductItem productItem)
    {
        return new ImportProductItem
        {
            //todo
        };
    }

    public static IImportCategoryItem ConvertToImportCategoryItem(this ICategory categoryItem)
    {
        return new ImportCategoryItem
        {
            //todo
        };
    }
}
