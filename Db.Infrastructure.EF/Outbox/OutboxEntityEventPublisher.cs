using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Db.Infrastructure.EF.Outbox;

internal class OutboxEntityEventPublisher : IOutboxEventPublisher
{
    public async Task Publish<T, TEvent>(TEvent notification, CancellationToken cancellationToken = default) where TEvent : OutboxEvent<T>
    {
        var jsonPayload = JsonSerializer.Serialize(notification.Entity);
        var eventType = notification.Entity.GetType().AssemblyQualifiedName;

        await  notification.DbContext.Database.ExecuteSqlRawAsync(
        "INSERT INTO message_entry (category, event_type, payload) VALUES ({0}, {1}, {2})",
        notification.EventName, eventType, jsonPayload);
    }
}