using Alchemist.Common;
using Db.Infrastructure;
using Message.Interfaces;

namespace Alchemist.Product.Import.DBService;

public class ImportItemHandlerService(ILogger<ImportItemHandlerService> logger,
    ICommandHandlerFactory commandHandlerFactory,
    IMessageReceiver messageReceiver,
    [FromKeyedServices("ImportEvents")] IDictionary<string, Type> events) : BackgroundService
{
    private readonly ILogger<ImportItemHandlerService> _logger = logger;

    private readonly ICommandHandlerFactory _commandHandlerFactory = commandHandlerFactory;

    private readonly IMessageReceiver _messageReceiver = messageReceiver;

    private readonly IDictionary<string, Type> _events = events;

    private readonly IDictionary<Type, ICommandHandler> _commandHandlers = new Dictionary<Type, ICommandHandler>();

    private async Task OnHandleItem(object item, CancellationToken cancellationToken)
    {
        try
        {
            if (item is not ICommand command) return;

            if (!_commandHandlers.TryGetValue(command.GetType(), out var commandHandler))
            {
                if (_commandHandlerFactory.GetHandler(command.GetType()) is ICommandHandler handler)
                    commandHandler = handler;

                else if (_commandHandlerFactory.GetHandler(command.GetType(), typeof(ItemProcessStatus)) is ICommandHandler resultHandler)
                    commandHandler = resultHandler;

                else
                {
                    _logger.LogWarning($"Handler for command type {command.GetType().Name} not found.");
                    return;
                }

                _commandHandlers.TryAdd(command.GetType(), commandHandler);
            }

            await commandHandler.Handle(item, cancellationToken);
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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _messageReceiver.Stop(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping event message receiver: {Message}", ex.Message);
        }

        await base.StopAsync(cancellationToken);
    }
}