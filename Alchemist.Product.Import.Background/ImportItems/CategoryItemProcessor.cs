using Alchemist.Common;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Product.Import.Background.Models;
using Alchemist.Product.Interfaces;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Product.Import.Background.ImportItems;

internal class CategoryItemProcessor(ILogger<CategoryItemProcessor> logger, 
    [FromKeyedServices(ShopImportWorkerKeys.ShopsMessageSenderKey)] IEnumerable<IMessageSender> itemMessageSenders)
    : ItemProcessor<IImportCategory, ImportCategory>(logger, Messages.Common.Messages.CategoryItem, itemMessageSenders)
{
    protected override ImportCategory CreateMessageItem(IImportCategory item, ResultStatus status)
    {
        var categoryModel = new ImportCategory { Category = item.Category.Name, ItemId = item.Category.Id, Status = status };

        if (item.CategoryShopModel is IShop shop) categoryModel.ShopId = shop.Id;

        return categoryModel;
    }

    protected override string GetInfo(ImportCategory item) => $"category {item.Name}";
}
