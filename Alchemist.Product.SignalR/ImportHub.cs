using Alchemist.Common;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Alchemist.Product.SignalR;

public class ImportHub : Hub
{
    public async Task SendProductItem(IImportProductItem productItemModel)
    {
        await Clients.All.SendAsync(Messages.ReceiveProductItem, productItemModel);
    }

    public async Task SendCategoryItem(ICategory categoryItemModel)
    {
        await Clients.All.SendAsync(Messages.ReceiveCategoryItem, categoryItemModel);
    }
}
