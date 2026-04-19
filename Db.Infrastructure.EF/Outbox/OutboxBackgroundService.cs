using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.Common;
using System.Text.Json;

namespace Db.Infrastructure.EF.Outbox;

internal class OutboxBackgroundService<TDbContext>(IServiceScopeFactory serviceScopeFactory) : BackgroundService
    where TDbContext:DbContext
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    private const int _daysToKeep = 1;

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
                var newEvents = await connection.QueryAsync<MessageEntry>(
                    "SELECT id, category, event_type as eventType, payload, created_at as createdAt FROM message_entry WHERE state = @p0 and processed_at is null",
                    new { p0 = "created" });

                transaction = await context.Database.BeginTransactionAsync(stoppingToken);

                foreach (var @event in newEvents)
                {
                    var (success, errors) = await ProcessMessageAsync(scope, @event, stoppingToken);
                    var aggregateException = errors.Length > 0 ? new AggregateException(errors) : null;
                    await UpdateEventStateAsync(connection, @event.Id, success ? "processed" : "failed", aggregateException?.Message);
                }

                await DoCleanupAsync(context, _daysToKeep);

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

    private async Task<(bool successed, Exception[])> ProcessMessageAsync(IServiceScope scope, MessageEntry messageEntry, CancellationToken cancellationToken = default)
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

            var messageHandleId = Guid.NewGuid().ToString();

            var @event = Activator.CreateInstance(eventType, messageHandleId, entity, messageEntry.Category, messageEntry.CreatedAt);

            if (handler is ICallback<string> callback)
                callback.Callback += EventHandleCallback;

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

    private void EventHandleCallback(object? sender, EventArgs<string> e)
    {
        //todo ack handler

        if (sender is ICallback<string> callback)
            callback.Callback -= EventHandleCallback;
    }

    private async Task UpdateEventStateAsync(DbConnection dbConnection, Guid id, string state, string? error)
    {
        string sql = "UPDATE message_entry SET state = @state, processed_at = @processedat, error=@error WHERE id = @id";

        int affectedRows = await dbConnection.ExecuteAsync(sql, new
        {
            state,
            id,
            processedat = DateTime.Now,
            error
        });
    }

    private static async Task DoCleanupAsync(TDbContext context, int daysToKeep)
    {
        string sql = "DELETE FROM message_entry WHERE created_at < @p0";
        DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);

        int rowsDeleted = await context.Database.ExecuteSqlRawAsync(sql, cutoffDate);
    }
}