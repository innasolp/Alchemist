using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Db.Infrastructure.EF.Outbox;

public static class AppExtensions
{
    public static async Task UseOutbox<TContext>(this IHost app, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {

            string tableSql = @"
            CREATE TABLE IF NOT EXISTS message_entry (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                event_type VARCHAR(255) NOT NULL,
                category VARCHAR(100) NOT NULL,
                payload TEXT NOT NULL,
                created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
                processed_at timestamp without time zone,
                state VARCHAR(32) NOT NULL DEFAULT 'created',
                error TEXT);";
            await context.Database.ExecuteSqlRawAsync(tableSql, cancellationToken);

            string indexSql = "CREATE INDEX IF NOT EXISTS ix_message_entry_category ON message_entry (category);";
            await context.Database.ExecuteSqlRawAsync(indexSql, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }
}