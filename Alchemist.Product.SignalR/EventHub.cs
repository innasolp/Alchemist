using Alchemist.Product.Entities;
using Alchemist.Import.Products.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Alchemist.Common;

namespace Alchemist.Product.SignalR;

public class EventHub : Hub
{
    public async Task SendShopCreated(Shop shop)
    {
        await Clients.All.SendAsync(Messages.ReceiveShopCreated, shop);
    }

    public async Task SendCategoryAdded(ShopCategory shopCategory)
    {
        await Clients.All.SendAsync(Messages.ReceiveCategoryAdded, shopCategory);
    }    

    public async Task SendProductItem(IImportProductItem importProduct)
    {
        await Clients.All.SendAsync(Messages.ReceiveProductItem, importProduct);
    }

    public async Task SendShopSettingsCreated(ShopSettings shopSettings)
    {
        await Clients.All.SendAsync(Messages.ReceiveShopSettingsCreated, shopSettings);
    }
}
