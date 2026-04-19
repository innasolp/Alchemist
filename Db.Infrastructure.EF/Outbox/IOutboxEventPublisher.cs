using Microsoft.EntityFrameworkCore;

namespace Db.Infrastructure.EF.Outbox;

internal class OutboxEvent<T>(T entity, string eventName, DateTime creationDate, DbContext dbContext) : Event<T>(entity, eventName, creationDate)
{
    public DbContext DbContext { get; } = dbContext;
}

internal interface IOutboxEventPublisher
{
    Task Publish<T, TEvent>(TEvent notification, CancellationToken cancellationToken = default)
        where TEvent : OutboxEvent<T>;
}