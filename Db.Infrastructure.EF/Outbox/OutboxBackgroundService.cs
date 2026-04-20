using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Db.Infrastructure.EF.Outbox;

internal class OutboxBackgroundService<TDbContext>(IServiceScopeFactory serviceScopeFactory) : BackgroundService
    where TDbContext:DbContext
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    private const int _daysToKeep = 1;

    private const int _hoursToKeepConfirmedMessages = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();   
            using var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            using var connection = context.Database.GetDbConnection();

            await connection.OpenAsync(stoppingToken);

            IDbContextTransaction? transaction = null;

            try
            {
                IEnumerable<MessageEntry> newEvents = await connection.GetNewEvents();

                transaction = await context.Database.BeginTransactionAsync(stoppingToken);

                foreach (var @event in newEvents)
                {
                    var (success, errors) = await ProcessMessageAsync(scope, context, @event, stoppingToken);
                    var aggregateException = errors.Length > 0 ? new AggregateException(errors) : null;
                    await context.UpdateEventStateAsync(@event.Id, success ? "Processed" : "Failed", aggregateException?.Message);
                }

                await context.ConfirmEventsIfNoProcessingHandlers();

                await context.DoCleanupAsync(_daysToKeep, _hoursToKeepConfirmedMessages);

                await transaction.CommitAsync(stoppingToken).ConfigureAwait(false);
            }
            catch
            {
                if (transaction != null)
                    await transaction.RollbackAsync(stoppingToken);

                throw;
            }
            finally
            {
                if(transaction != null)                
                    await transaction.DisposeAsync();                

                await connection.CloseAsync();
            }
        }
    }    

    private async Task<(bool successed, Exception[])> ProcessMessageAsync(IServiceScope scope, TDbContext dbContext, MessageEntry messageEntry, CancellationToken cancellationToken = default)
    {
        var entityType = Type.GetType(messageEntry.EventType)
            ?? throw new ArgumentException($"Message type {messageEntry.EventType} not found.", nameof(messageEntry));

        var entity = JsonSerializer.Deserialize(messageEntry.Payload, entityType);

        var eventType = typeof(IdentifiedEvent<>).MakeGenericType(entityType); 
        var handlerType = typeof(IEventHandler<,>).MakeGenericType(entityType,eventType);

        var handlers = scope.ServiceProvider.GetServices(handlerType).ToList();
        var errors = new List<Exception>();

        foreach (var handler in handlers)
        {
            var method = handlerType
            .GetInterfaces()
            .Append(typeof(INotificationHandler<>)) // Добавляем сам интерфейс в список поиска
            .Select(i => i.GetMethod("Handle"))
            .FirstOrDefault(m => m != null);

            var messageHandleId = Guid.NewGuid();

            var @event = Activator.CreateInstance(eventType, messageHandleId.ToString(), entity, messageEntry.Category, messageEntry.CreatedAt);

            if (handler is ICallback<string> callback)
            {
                await dbContext.CreateMessageHandlerEntryAsync( 
                    new MessageHandlerEntry { Id = messageHandleId, MessageId = messageEntry.Id });

                callback.Callback += EventHandleCallback;
            }

            var handle = method!.Invoke(handler, [@event, cancellationToken]);
            try
            {
                await (Task)handle!;                
            }
            catch(Exception ex) 
            {
                errors.Add(ex);
            }
        }

        return (errors.Count < handlers.Count, errors.ToArray());
    }    

    private  Task UpdateMessageHandlerEntryAsync(Guid id, string state, string? error = null)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        return context.UpdateMessageHandlerEntryAsync(id, state, error);
    }

    private async Task EventHandleCallback(object? sender, CallbackAsyncEventArgs<string> e)
    {
        await UpdateMessageHandlerEntryAsync(Guid.Parse(e.Value),
            e.Exception == null ? State.Confirmed.ToString() : State.Processing.ToString(), 
            e.Exception?.Message);

        if (sender is ICallback<string> callback && e.Exception == null)
            callback.Callback -= EventHandleCallback;
    }  
}