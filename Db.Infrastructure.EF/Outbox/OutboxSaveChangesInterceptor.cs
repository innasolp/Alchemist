using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Db.Infrastructure.EF.Outbox;

public record SupportedEventType(string EventName, int EntityState, string EntityType);

internal class OutboxSaveChangesInterceptor(IReadOnlyList<SupportedEventType> supportedTypes, IEntityEventPublisher eventPublisher)
    : SaveChangesInterceptor
{
    private readonly IReadOnlyList<SupportedEventType> _supportedTypes = supportedTypes;

    private readonly IEntityEventPublisher _eventPublisher = eventPublisher;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Join(_supportedTypes, 
                    e=> (e.State, e.Entity.GetType().Name), 
                    st=>((EntityState)st.EntityState, st.EntityType), 
                (e, st)=>new { e.Entity, Event = st.EventName }).ToList();

        foreach(var entry in entries)
        {
            await _eventPublisher.Publish<object, Event<object>>(new Event<object>(entry.Entity, entry.Event, DateTime.Now), cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}