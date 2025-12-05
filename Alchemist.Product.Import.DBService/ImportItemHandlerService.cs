using Alchemist.Common;
using Alchemist.Product.DbItemHandler;
using Message.Interfaces;
using System.Collections.Concurrent;

namespace Alchemist.Product.Import.DBService;

public class ImportItemHandlerService(ILogger<ImportItemHandlerService> logger,
    IMessageReceiver messageReceiver, IEnumerable<IImportItemHandler> importItemHandlers) : BackgroundService
{
    private readonly ILogger<ImportItemHandlerService> _logger = logger;

    private readonly IMessageReceiver _messageReceiver = messageReceiver;

    private readonly IEnumerable<IImportItemHandler> _importItemHandlers = importItemHandlers;

    private readonly ConcurrentDictionary<Type, IImportItemHandler> _typedItemHandlers = new();

    private async Task OnHandleItem(object item, CancellationToken cancellationToken)
    {
        try
        {
            if (!_typedItemHandlers.TryGetValue(item.GetType(), out var handler))
            {
                handler = _importItemHandlers.FirstOrDefault(h => h.ItemType == item.GetType() || item.GetType().IsImplementation(h.ItemType));
                if (handler != null)
                    _typedItemHandlers.TryAdd(item.GetType(), handler);
            }

            if (handler != null)
                await handler.HandleItem(item, cancellationToken);
        }
        catch(OperationCanceledException)
        {
            _logger.LogInformation("Item handling canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Item handling error.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task onHandleItemTask(object item) => OnHandleItem(item, stoppingToken);

        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_messageReceiver.IsConnected)
                    continue;

                await _messageReceiver.Start(stoppingToken);

                _logger.LogInformation("Import service connected to messaging host.");

                foreach (var itemHandler in _importItemHandlers)
                    _messageReceiver.On(itemHandler.EventName, onHandleItemTask, itemHandler.ItemType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}