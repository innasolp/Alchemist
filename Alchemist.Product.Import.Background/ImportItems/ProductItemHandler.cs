using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.Import.Background.Models;
using Message.Interfaces;


namespace Alchemist.Product.Import.Background.ImportItems;

internal class ProductItemHandler(IMessageSender messageSender, string methodName, ProductItemProcessor productItemProcessor)
    : ItemHandler<IImportProduct, ImportItem.Interfaces.IProductData, ImportProduct>(messageSender, methodName, productItemProcessor), IProductItemHandler
{
    protected override ImportItem.Interfaces.IProductData ConvertToImportEntity(IImportProduct item)
    {
        return item.ProductItem.ConvertToImportProductItem(item.Shop);
    }

    protected override string GetUrl(IImportProduct item)
    {
        return item.ProductItem.Url;
    }
}
