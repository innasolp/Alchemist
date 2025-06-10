using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Alchemist.Product.Data;

public class DateChangedInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;
        await SetDates(context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static async Task SetDates(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added && e.Entity is IAddedTsEnity).
            Select(e => e.Entity).OfType<IAddedTsEnity>())
        {
            entry.AddedTs = DateTime.Now;
        }

        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified && e.Entity is IUpdatedTsEntity entity))           
        {
            if(entry.Entity is IAddedTsEnity addedTsEnity && (await entry.GetDatabaseValuesAsync())?["AddedTs"] is DateTime addedTs)            
                addedTsEnity.AddedTs = addedTs;            
            
            (entry.Entity as IUpdatedTsEntity).UpdatedTs = DateTime.Now;            
        }
    }
}
