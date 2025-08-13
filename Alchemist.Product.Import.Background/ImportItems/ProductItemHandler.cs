using Alchemist.Import.Products.Interfaces;
using Message.Interfaces;


namespace Alchemist.Product.Import.Background.ImportItems;

internal class ProductItemHandler(IMessageSender messageSender, string methodName)
    : ItemHandler<IImportProduct, ImportItem.Interfaces.IProductData>(messageSender, methodName), IProductItemHandler
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
