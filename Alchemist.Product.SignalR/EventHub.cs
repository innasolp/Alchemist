using Alchemist.Product.Entities;
using Alchemist.Import.Products.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Alchemist.Product.SignalR;

public class EventHub : Hub
{
    public async Task SendShopCreated(Shop shop)
    {
        await Clients.All.SendAsync(Messages.Common.Messages.ReceiveShopCreated, shop);
    }

    public async Task SendCategoryAdded(ShopCategory shopCategory)
    {
        await Clients.All.SendAsync(Messages.Common.Messages.ReceiveCategoryAdded, shopCategory);
    } 

    public async Task SendShopSettingsCreated(ShopSettings shopSettings)
    {
        await Clients.All.SendAsync(Messages.Common.Messages.ReceiveShopSettingsCreated, shopSettings);
    }

    public async Task SendServiceStart(Guid guid)
    {
        await Clients.All.SendAsync(Messages.Common.Messages.ReceiveServiceStart, guid);
    }

    public async Task SendServiceStop(Guid guid)
    {
        await Clients.All.SendAsync(Messages.Common.Messages.ReceiveServiceStop, guid);
    }

    public async Task SendServiceCreated(Guid guid)
    {
        await Clients.All.SendAsync(Messages.Common.Messages.ReceiveServiceCreated, guid);
    }
}
