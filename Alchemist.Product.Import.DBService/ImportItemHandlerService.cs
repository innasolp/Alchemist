using MediatR;
using Message.Interfaces;

namespace Alchemist.Product.Import.DBService;

public class ImportItemHandlerService(ILogger<ImportItemHandlerService> logger, 
    IMediator mediator,
    IMessageReceiver messageReceiver,
    [FromKeyedServices("ImportEvents")] IDictionary<string, Type> events) : BackgroundService
{
    private readonly ILogger<ImportItemHandlerService> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IMessageReceiver _messageReceiver = messageReceiver;
    private readonly IDictionary<string, Type> _events = events; 

    private async Task OnHandleItem(object item, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(item, cancellationToken);
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

                foreach (var @event in _events)
                    _messageReceiver.On(@event.Key, onHandleItemTask, @event.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}