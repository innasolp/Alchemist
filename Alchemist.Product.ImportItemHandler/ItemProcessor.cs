using Alchemist.Common;
using Message.Interfaces;
using Microsoft.Extensions.Logging;


namespace Alchemist.Product.Import.Background.ImportItems;

internal abstract class ItemProcessor<TProcessItem, TMessageItem>(ILogger logger, string eventName, IEnumerable<IMessageSender> itemMessageSenders)
{
    private readonly IEnumerable<IMessageSender> _itemMessageSenders = itemMessageSenders;

    private readonly ILogger _logger = logger;

    private readonly string _eventName = eventName;

    public async Task ProcessItemAsync(TProcessItem item, ResultStatus status, CancellationToken cancellationToken = default)
    {
        var messageItem = CreateMessageItem(item, status);
        await SendItemMessagesAsync(messageItem, _eventName, cancellationToken);
    }

    protected abstract TMessageItem CreateMessageItem(TProcessItem item, ResultStatus status);

    protected abstract string GetInfo(TMessageItem item);

    private async Task SendItemMessagesAsync(TMessageItem item, string eventName, CancellationToken cancellationToken = default)        
    {
        foreach (var itemMessageSender in _itemMessageSenders)
        {
            try
            {
                if (!itemMessageSender.IsConnected)
                    await itemMessageSender.Start(cancellationToken);

                await itemMessageSender.Send(item, eventName, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Item {GetInfo(item)} handling for event {eventName} failed in message sender {itemMessageSender.GetType()}.");
            }
        }
    }
}
