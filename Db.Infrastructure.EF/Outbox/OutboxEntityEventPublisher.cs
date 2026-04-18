using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Db.Infrastructure.EF.Outbox;

internal class OutboxEntityEventPublisher(IServiceScopeFactory serviceScopeFactory) : IEntityEventPublisher
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Publish<T, TEvent>(TEvent notification, CancellationToken cancellationToken = default) where TEvent : IEvent<T>
    {
        using var scope = _serviceScopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

        var jsonPayload = JsonSerializer.Serialize(notification.Entity);
        var eventType = notification.Entity.GetType().AssemblyQualifiedName;

        await  dbContext.Database.ExecuteSqlRawAsync(
        "INSERT INTO message_entry (category, event_type, payload) VALUES ({0}, {1}, {2})",
        notification.EventName, eventType, jsonPayload);
    }
}