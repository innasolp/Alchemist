using Alchemist.Common;
using Alchemist.Product.ImportItem.Handler;
using Message.Interfaces;
using System.Collections.Concurrent;

namespace Alchemist.Product.Import.DBService;

public class ImportItemHandlerService(ILogger<ImportItemHandlerService> logger,
    IMessageReceiver messageReceiver, IEnumerable<IImportItemHandler> importItemHandlers
    ) : BackgroundService
{
    private readonly ILogger<ImportItemHandlerService> _logger = logger;

    private readonly IMessageReceiver _messageReceiver = messageReceiver;

    private readonly IEnumerable<IImportItemHandler> _importItemHandlers = importItemHandlers;

    private readonly ConcurrentDictionary<Type, IImportItemHandler> _typedItemHandlers = new();

    private async Task OnHandleItem<T>(T item)
    {
        if (!_typedItemHandlers.TryGetValue(item.GetType(), out var handler))
        {
            handler = _importItemHandlers.FirstOrDefault(h=>h.ItemType == item.GetType() || item.GetType().IsImplementation(h.ItemType));
            if(handler != null)
                _typedItemHandlers.TryAdd(item.GetType(), handler);
        }
        
        if (handler != null)
            await handler.HandleItem(item);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {   
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_messageReceiver.IsConnected) continue;

                await _messageReceiver.Start();

                _logger.LogInformation("Import service connected to messaging host.");

                foreach(var itemHandler in _importItemHandlers)
                    _messageReceiver.On(itemHandler.EventName, OnHandleItem, itemHandler.ItemType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}
