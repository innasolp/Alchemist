using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.ImportItem.Interfaces;
using Message.Interfaces;


namespace Alchemist.Product.Import.Background.ImportItems;

internal class ProductItemHandler(IMessageSender messageSender, string methodName)
    : ItemHandler<IProductItem, IImportProductItem>(messageSender, methodName), IProductItemHandler
{
    protected override IImportProductItem ConvertToImportEntity(IProductItem item)
    {
        return item.ConvertToImportProductItem();
    }
}
