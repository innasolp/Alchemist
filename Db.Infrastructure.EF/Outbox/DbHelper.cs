using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Db.Infrastructure.EF.Outbox;

internal static class DbHelper
{
    public static async Task CreateMessageEntryTableIfNotExistsAsync<TContext>(this TContext context, CancellationToken cancellationToken)
        where TContext : DbContext
    {
        string tableSql = @"
            CREATE TABLE IF NOT EXISTS message_entry (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                event_type VARCHAR(255) NOT NULL,
                category VARCHAR(100) NOT NULL,
                payload TEXT NOT NULL,
                created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
                processed_at timestamp without time zone,
                state VARCHAR(32) NOT NULL DEFAULT 'Created');";
        await context.Database.ExecuteSqlRawAsync(tableSql, cancellationToken);

        string indexSql = "CREATE INDEX IF NOT EXISTS ix_message_entry_category ON message_entry (category);";
        await context.Database.ExecuteSqlRawAsync(indexSql, cancellationToken);
    }

    public static async Task CreateMessageHandlerTableIfNotExistsAsync<TContext>(this TContext context, CancellationToken cancellationToken)
        where TContext : DbContext
    {
        string tableSql = @"
            CREATE TABLE IF NOT EXISTS message_handler (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                message_id UUID NOT NULL,       
                handler_type VARCHAR(512) NOT NULL,
                created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
                processed_at timestamp without time zone,
                state VARCHAR(32) NOT NULL DEFAULT 'Created',
                error TEXT);";
        await context.Database.ExecuteSqlRawAsync(tableSql, cancellationToken);
    }

    public static async Task<IEnumerable<MessageEntry>> GetNewEvents(this DbConnection connection)
    {
        return await connection.QueryAsync<MessageEntry>(
            "SELECT id, category, event_type as eventType, payload, created_at as createdAt FROM message_entry WHERE state = @p0 and processed_at is null",
            new { p0 = "Created" });
    }

    public static async Task UpdateMessageHandlerEntryAsync(this DbContext context, Guid id, string state, string? error = null)
    {
        await context.Database.ExecuteSqlRawAsync(
        "UPDATE message_handler SET state = @p0, processed_at = @p1, error = @p2 WHERE id = @p3",
            state,
            DateTime.Now,
            error ?? "",
            id
        );
    }
    public static async Task UpdateEventStateAsync(this DbContext context, Guid id, string state)
    {
        string sql = "UPDATE message_entry SET state = @p0, processed_at = @p1 WHERE id = @p2";

        int affectedRows = await context.Database.ExecuteSqlRawAsync(sql,
            state,
            DateTime.Now,
            id
        );
    }

    public static async Task DoCleanupAsync(this DbContext context, int hoursToKeepConfirmedMessages)
    {
        string sql = "DELETE FROM message_entry WHERE created_at < @p0 and state = 'Confirmed'";
        DateTime cutoffDateForConfirm = DateTime.Now.AddHours(-hoursToKeepConfirmedMessages);
        int rowsDeleted = await context.Database.ExecuteSqlRawAsync(sql, cutoffDateForConfirm);

        sql = "DELETE FROM message_handler WHERE created_at < @p0 and state = 'Confirmed'";
        cutoffDateForConfirm = DateTime.Now.AddHours(-hoursToKeepConfirmedMessages);
        rowsDeleted = await context.Database.ExecuteSqlRawAsync(sql, cutoffDateForConfirm);
    }

    public static Task ConfirmEventsIfNoProcessingHandlers(this DbContext context)
    {
        string sql = @"UPDATE message_entry m
                        SET state = 'Confirmed',
                            processed_at = @p0
                        FROM message_entry m2
                        LEFT JOIN message_handler mh 
                            ON mh.message_id = m2.id 
                            AND mh.state <> 'Confirmed'
                        WHERE m.id = m2.id
                          AND mh.id IS NULL
                        AND m.state = 'Processing'";
        return context.Database.ExecuteSqlRawAsync(sql, DateTime.Now);
    }

    public static Task CreateMessageHandlerEntryAsync(this DbContext dbContext, MessageHandlerEntry messageHandlerEntry)
    {
        return dbContext.Database.ExecuteSqlRawAsync(
        "INSERT INTO message_handler (id, message_id, handler_type) VALUES ({0}, {1}, {2})",
        messageHandlerEntry.Id, messageHandlerEntry.MessageId, messageHandlerEntry.HandlerType);
    }
}