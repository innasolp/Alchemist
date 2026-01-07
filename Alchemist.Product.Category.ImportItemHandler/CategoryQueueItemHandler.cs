using Alchemist.BackgroundTaskQueue;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Product.CategoryData;
using Alchemist.Product.Entities;
using Alchemist.Product.ImportItemHandler;
using Message.Interfaces;

namespace Alchemist.Product.Category.ImportItemHandler;

internal class CategoryQueueItemHandler(IBackgroundTaskQueue backgroundTaskQueue, IMessageSender messageSender, string methodName) 
    : QueueItemHandler<IImportCategory, ICategoryData>(backgroundTaskQueue, messageSender, methodName), ICategoryItemHandler
{
    protected override async Task<ICategoryData> ConvertToImportEntity(IImportCategory item)
    {
        return ConvertToImportCategoryItem(item.Category, item.SourceName, item.SourceUrl);
    }

    protected override string GetUrl(IImportCategory item)
    {
        return item.Category.Url;
    }

    private static ICategoryData ConvertToImportCategoryItem(ICategory categoryItem, string shopName, string shopUrl)
    {
        return new CategoryData.CategoryData
        {
            ParentCategory = categoryItem.ItemParent != null
                ? new ShopCategory { Category = categoryItem.ItemParent.Name, ItemId = categoryItem.ItemParent.Id, Url = categoryItem.ItemParent.Url }
                : null,
            ShopCategory = new ShopCategory { Category = categoryItem.Name, ItemId = categoryItem.Id, Url = categoryItem.Url  },
            ShopName = shopName,
            ShopUrl = shopUrl
        };
    }
}