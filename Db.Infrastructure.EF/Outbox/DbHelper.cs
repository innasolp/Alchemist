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
                handler_type VARCHAR(1024) NOT NULL,
                created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
                processed_at timestamp without time zone,
                state VARCHAR(32) NOT NULL DEFAULT 'Created',
                error TEXT,
                retry_count INTEGER NOT NULL DEFAULT 0);";
        await context.Database.ExecuteSqlRawAsync(tableSql, cancellationToken);
    }

    public static async Task<IEnumerable<MessageEntry>> GetNeedForHandleEvents(this DbConnection connection)
    {
        return await connection.QueryAsync<MessageEntry>(
            @"SELECT id, category, event_type as eventType, payload, created_at as createdAt 
            FROM message_entry 
            WHERE state <> @p0 and state <> @p1",
            new { p0 = State.Confirmed.ToString(), p1 = State.Failed.ToString() });
    }

    public static async Task UpdateMessageHandlerEntryAsync(this DbContext context, Guid id, string state, string? error = null)
    {
        const string sql = @"UPDATE message_handler SET state = @p0, processed_at = @p1, error = @p2, 
            retry_count = (case when @p2 is not null AND @p2 <> '' THEN retry_count+1 ELSE retry_count end)    
            WHERE id = @p3";

        await context.Database.ExecuteSqlRawAsync(sql,
            state,
            DateTime.Now,
            error ?? "",
            id
        );
    }

    public static async Task<IEnumerable<MessageHandlerEntry>> GetEventMessageHandlerAsync(this DbConnection connection, Guid messageId)
    {
        const string sql = @"SELECT id, 
                            message_id as MessageId,  
                            handler_type as HandlerType, 
                            processed_at as ProcessedAt,  
                            created_at as createdAt,
                            state, 
                            error, 
                            retry_count as RetryCount 
                            FROM message_handler   
                            WHERE message_id = @p0";

        return await connection.QueryAsync<MessageHandlerEntry>(sql,new { p0 = messageId });
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
        const string sql = @"UPDATE message_entry m 
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

    public static Task CreateMessageHandlerEntryIfNotExistsAsync(this DbContext dbContext, MessageHandlerEntry messageHandlerEntry)
    {
        return dbContext.Database.ExecuteSqlRawAsync(
        @"INSERT INTO message_handler (id, message_id, handler_type) 
            SELECT @p0, @p1, @p2
            WHERE NOT EXISTS ( 
                SELECT 1 FROM message_handler 
                WHERE message_id = @p1 AND handler_type = @p2);",
        messageHandlerEntry.Id, messageHandlerEntry.MessageId, messageHandlerEntry.HandlerType);
    }
}