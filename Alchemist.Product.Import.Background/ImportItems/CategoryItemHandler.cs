using Alchemist.Import.Category.Interfaces;
using Alchemist.Product.DataItem.Interfaces;
using Message.Interfaces;

namespace Alchemist.Product.Import.Background.ImportItems;

internal class CategoryItemHandler(IMessageSender messageSender, string methodName)
    : ItemHandler<IImportCategory, ICategoryData>(messageSender, methodName), ICategoryItemHandler
{
    protected override ICategoryData ConvertToImportEntity(IImportCategory item)
    {
        return item.Category.ConvertToImportCategoryItem(item.CategoryShopModel);
    }

    protected override string GetUrl(IImportCategory item)
    {
        return item.Category.Url;
    }
}
