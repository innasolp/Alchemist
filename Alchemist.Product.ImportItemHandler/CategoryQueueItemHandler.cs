using Alchemist.BackgroundTaskQueue;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Product.DataItem.Interfaces;
using Message.Interfaces;

namespace Alchemist.Product.ImportItemHandler;

internal class CategoryQueueItemHandler(IBackgroundTaskQueue backgroundTaskQueue, IMessageSender messageSender, string methodName) 
    : QueueItemHandler<IImportCategory, ICategoryData>(backgroundTaskQueue, messageSender, methodName), ICategoryItemHandler
{
    protected override ICategoryData ConvertToImportEntity(IImportCategory item)
    {
        return item.Category.ConvertToImportCategoryItem(item.SourceName, item.SourceUrl);
    }

    protected override string GetUrl(IImportCategory item)
    {
        return item.Category.Url;
    }
}