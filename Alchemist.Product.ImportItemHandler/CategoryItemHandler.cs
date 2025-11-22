using Alchemist.Import.Category.Interfaces;
using Alchemist.Product.DataItem.Interfaces;
using Alchemist.Product.Import.Background.ImportItems;
using Message.Interfaces;

namespace Alchemist.Product.ImportItemHandler;

internal class CategoryItemHandler(IMessageSender messageSender, string methodName, CategoryItemProcessor categoryItemProcessor)
    : ItemHandler<IImportCategory, ICategoryData, ImportCategory>(messageSender, methodName, categoryItemProcessor), ICategoryItemHandler
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
