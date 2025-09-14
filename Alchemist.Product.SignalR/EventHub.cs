using Alchemist.Product.Entities;
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

    public async Task SendServiceCreated(object[] parameters)
    {
        if (parameters.Length < 2)
            throw new Exception($"Invalid arguments for event {nameof(SendServiceCreated)} : {string.Join(";", parameters)}.");

        if (!Guid.TryParse(parameters[0].ToString(),out var guid))
            throw new Exception($"Value {parameters[0]} is invalid for parameter guid for event {nameof(SendServiceCreated)}.");

        var name = parameters[1]?.ToString();
        if (string.IsNullOrEmpty(name))
            throw new Exception($"Value of parameter {nameof(name)} for event {nameof(SendServiceCreated)} must be not empty");

        await Clients.All.SendAsync(Messages.Common.Messages.ReceiveServiceCreated, new object[] { guid, name });
    }

    public async Task SendServiceStarted(Guid guid)
    {
        await Clients.All.SendAsync(Messages.Common.Messages.ReceiveServiceStarted, guid);
    }

    public async Task SendServiceStopped(Guid guid)
    {
        await Clients.All.SendAsync(Messages.Common.Messages.ReceiveServiceStopped, guid);
    }

    public async Task SendServiceEventError(object[] parameters)
    {
        if (parameters.Length < 4)
            throw new Exception($"Invalid arguments for event {nameof(SendServiceEventError)} : {string.Join(";", parameters)}.");

        if (!Guid.TryParse(parameters[0].ToString(), out var guid))
            throw new Exception($"Value {parameters[0]} is invalid for parameter guid for event {nameof(SendServiceEventError)}.");

        var name = parameters[1]?.ToString();

        var eventName = parameters[2]?.ToString();
        if(string.IsNullOrEmpty(eventName))
            throw new Exception($"Value of parameter {nameof(eventName)} for event {nameof(SendServiceEventError)} must be not empty");

        var errorMessage = parameters[3]?.ToString();

        if (parameters.Length > 4 && parameters[4] is Exception exception)  
            await Clients.All.SendAsync(Messages.Common.Messages.ReceiveServiceEventError, new object[] { guid, name, eventName, errorMessage, exception } );
        else
            await Clients.All.SendAsync(Messages.Common.Messages.ReceiveServiceEventError, new object[] { guid, name, eventName, errorMessage });
    }

}
