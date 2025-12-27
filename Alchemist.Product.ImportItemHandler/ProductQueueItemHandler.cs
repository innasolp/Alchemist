using Alchemist.BackgroundTaskQueue;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.DataItem.Interfaces;
using Message.Interfaces;

namespace Alchemist.Product.ImportItemHandler;

internal class ProductQueueItemHandler(IBackgroundTaskQueue backgroundTaskQueue, IMessageSender messageSender, string methodName) 
    : QueueItemHandler<IImportProduct, IProductData>(backgroundTaskQueue, messageSender, methodName), IProductItemHandler
{
    protected override IProductData ConvertToImportEntity(IImportProduct item)
    {
        return item.ProductItem.ConvertToImportProductItem(item.SourceName, item.SourceUrl);
    }

    protected override string GetUrl(IImportProduct item)
    {
        return item.ProductItem.Url;
    }
}