using Alchemist.Import.Products.Interfaces;
using Message.Interfaces;


namespace Alchemist.Product.ImportItemHandler;

internal class ProductItemHandler(IMessageSender messageSender, string methodName, ProductItemProcessor productItemProcessor)
    : ItemHandler<IImportProduct, ImportItem.Interfaces.IProductData, ImportProduct>(messageSender, methodName, productItemProcessor), IProductItemHandler
{
    protected override ImportItem.Interfaces.IProductData ConvertToImportEntity(IImportProduct item)
    {
        return item.ProductItem.ConvertToImportProductItem(item.SourceName, item.SourceUrl);
    }

    protected override string GetUrl(IImportProduct item)
    {
        return item.ProductItem.Url;
    }
}