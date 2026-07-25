using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace Db.Infrastructure.EF.Outbox;

internal class OutboxBackgroundService<TDbContext>(ILogger<OutboxBackgroundService<TDbContext>> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
    where TDbContext : DbContext
{
    private readonly ILogger<OutboxBackgroundService<TDbContext>> _logger = logger;

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    private const int _hoursToKeepConfirmedMessages = 1;

    private const int MaxHandleRetryCount = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            
            using var connection = context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(stoppingToken);
                }

                IEnumerable<MessageEntry> needInHandlingEvents = await connection.GetNeedForHandleEvents();

                foreach (var @event in needInHandlingEvents)
                {
                    using var handleTransaction = await context.Database.BeginTransactionAsync(stoppingToken);
                    try
                    {
                        await ProcessMessageAsync(scope, context, connection, @event, stoppingToken);
                        await context.UpdateEventStateAsync(@event.Id, State.Processing.ToString(), stoppingToken);
                        await handleTransaction.CommitAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Handling message failed");

                        await handleTransaction.RollbackAsync(stoppingToken);
                    }
                }

                using var processingTransaction = await context.Database.BeginTransactionAsync(stoppingToken);
                try
                {
                    await context.ConfirmEventsIfNoProcessingHandlers(stoppingToken);

                    await context.DoCleanupAsync(_hoursToKeepConfirmedMessages, stoppingToken);

                    await processingTransaction.CommitAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Processing message statuses failed");

                    await processingTransaction.RollbackAsync(stoppingToken);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Outbox background error");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(IServiceScope scope, TDbContext dbContext, DbConnection dbConnection, MessageEntry messageEntry, CancellationToken cancellationToken = default)
    {
        var entityType = Type.GetType(messageEntry.EventType)
            ?? throw new ArgumentException($"Message type {messageEntry.EventType} not found.", nameof(messageEntry));

        var entity = JsonSerializer.Deserialize(messageEntry.Payload, entityType);

        var eventType = typeof(IdentifiedEvent<>).MakeGenericType(entityType);
        var handlerType = typeof(IEventHandler<,>).MakeGenericType(entityType, eventType);

        var handlers = scope.ServiceProvider.GetServices(handlerType).ToList();

        var messageHandlers = await dbConnection.GetEventMessageHandlersAsync(messageEntry.Id);

        if (handlers.Count > 0)
            foreach (var handler in handlers)
            {
                var messageHandler = messageHandlers.FirstOrDefault(mh => mh.HandlerType == handler.GetType().FullName);

                if (messageHandler == null || (messageHandler.State == State.Failed.ToString() && messageHandler.RetryCount < MaxHandleRetryCount))
                {
                    var messageHandlerId = messageHandler?.Id ?? Guid.NewGuid();

                    if(messageHandler == null)
                        await dbContext.CreateMessageHandlerEntryIfNotExistsAsync(
                        new MessageHandlerEntry { Id = messageHandlerId,
                            MessageId = messageEntry.Id, 
                            HandlerType = handler.GetType().FullName }, cancellationToken);

                    await HandleMessage(messageEntry, entity, eventType, handlerType, handler, messageHandlerId, cancellationToken);
                }
            }
        else
            await dbContext.UpdateEventStateAsync(messageEntry.Id, State.Confirmed.ToString(), cancellationToken);
    }

    private async Task HandleMessage(MessageEntry messageEntry, object? entity, Type eventType, Type handlerType, object? handler, Guid messageHandlerId, CancellationToken cancellationToken)
    {
        var method = handlerType
                    .GetInterfaces()
                    .Append(typeof(INotificationHandler<>)) // Добавляем сам интерфейс в список поиска
                    .Select(i => i.GetMethod("Handle"))
                    .FirstOrDefault(m => m != null);        

        var @event = Activator.CreateInstance(eventType, messageHandlerId.ToString(), entity, messageEntry.Category, messageEntry.CreatedAt);
                
        if (handler is ICallback<string> callback)
            callback.Callback += EventHandleCallback;

        var handle = method!.Invoke(handler, [@event, cancellationToken]);
        try
        {
            await (Task)handle!;

            var state = handler is ICallback<string> ? State.Processing.ToString() : State.Confirmed.ToString();
            await UpdateMessageHandlerEntryAsync(messageHandlerId, state, cancellationToken : cancellationToken);
        }
        catch (Exception ex)
        {
            await UpdateMessageHandlerEntryAsync(messageHandlerId, State.Failed.ToString(), ex.Message, cancellationToken);
        }
    }

    private async Task UpdateMessageHandlerEntryAsync(Guid id, string state, string? error = null, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await context.UpdateMessageHandlerEntryAsync(id, state, error, cancellationToken);
    }

    private async Task EventHandleCallback(object? sender, CallbackAsyncEventArgs<string> e)
    {
        try
        {
            var state = e.Exception == null ? State.Confirmed.ToString() : State.Failed.ToString();
            await UpdateMessageHandlerEntryAsync(Guid.Parse(e.Value),
                state,
                e.Exception?.Message, e.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Acknowledgement request {e.Value} failed.");
        }

        if (sender is ICallback<string> callback && e.Exception == null)
            callback.Callback -= EventHandleCallback;
    }
}