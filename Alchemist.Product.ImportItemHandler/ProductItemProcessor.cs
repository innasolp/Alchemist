using Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Product.ImportItemHandler;

internal class ProductItemProcessor(ILogger<ProductItemProcessor> logger,
    [FromKeyedServices(ServiceKeys.ImportProductMessageSenderKey)] IEnumerable<IMessageSender> itemMessageSenders)
    : ItemProcessor<IImportProduct, ImportProduct>(logger, Messages.Common.Messages.ProductItem, itemMessageSenders)
{
    protected override ImportProduct CreateMessageItem(IImportProduct item, ResultStatus status)
    {
        return new ImportProduct { Name = item.ProductItem.Name, 
            ShopName = item.SourceName, 
            ShopUrl = item.SourceUrl, 
            Url = item.ProductItem.Url, 
            Status = status };
    }

    protected override string GetInfo(ImportProduct item) => $"product {item.Name}";
}