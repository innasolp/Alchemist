using Alchemist.Common;
using Message.Interfaces;
using Microsoft.Extensions.Logging;


namespace Alchemist.Product.Import.Background.ImportItems;

internal abstract class ItemProcessor<TProcessItem, TMessageItem>(ILogger logger, string eventName, IEnumerable<IMessageSender> itemMessageSenders)
{
    private readonly IEnumerable<IMessageSender> _itemMessageSenders = itemMessageSenders;

    private readonly ILogger _logger = logger;

    private readonly string _eventName = eventName;

    public async Task ProcessItemAsync(TProcessItem item, ResultStatus status)
    {
        var messageItem = CreateMessageItem(item, status);
        await SendItemMessagesAsync(messageItem, _eventName);
    }

    protected abstract TMessageItem CreateMessageItem(TProcessItem item, ResultStatus status);

    protected abstract string GetInfo(TMessageItem item);

    private async Task SendItemMessagesAsync(TMessageItem item, string eventName)        
    {
        foreach (var itemMessageSender in _itemMessageSenders)
        {
            try
            {
                if (!itemMessageSender.IsConnected)
                    await itemMessageSender.Start();

                await itemMessageSender.Send(item, eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Item {GetInfo(item)} handling for event {eventName} failed in message sender {itemMessageSender.GetType()}.");
            }
        }
    }
}
