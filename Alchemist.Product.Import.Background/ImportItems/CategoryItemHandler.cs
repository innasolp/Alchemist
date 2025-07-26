using Alchemist.Import.Category.Interfaces;
using Alchemist.Product.ImportItem.Interfaces;
using Message.Interfaces;

namespace Alchemist.Product.Import.Background.ImportItems;

internal class CategoryItemHandler(IMessageSender messageSender, string methodName)
    : ItemHandler<ICategory, IImportCategoryItem>(messageSender, methodName), ICategoryItemHandler
{
    protected override IImportCategoryItem ConvertToImportEntity(ICategory item)
    {
        return item.ConvertToImportCategoryItem();
    }
}
