using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace DbMigration.Postgres;

public static  class PostgresContextAppExtensions
{
    public static async Task UsePostgresqlMigrationAsync<TContext>(this IHost app)
        where TContext:DbContext
    {
        using var scope = app.Services.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TContext>>();

        using var context = factory.CreateDbContext();

        try
        {
            await MigratePostgresIfNeedAsync(context);
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P04" || ex.SqlState == "55P03")
        {}        
    }

    public static async Task UseAlchemyPostgresqlMigrationWithRedisLockAsync<TContext>(this IHost app, string redisConnectionString,
        int expirySeconds = 60, int waitSeconds = 60, int retrySeconds = 1)
        where TContext : DbContext
    {        
        TimeSpan expiry = TimeSpan.FromSeconds(expirySeconds);
        TimeSpan wait = TimeSpan.FromSeconds(waitSeconds);
        TimeSpan retry = TimeSpan.FromSeconds(retrySeconds);

        using var redisMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

        var db = redisMultiplexer.GetDatabase();

        using var scope = app.Services.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TContext>>();

        using var context = factory.CreateDbContext();

        var builder = new Npgsql.NpgsqlConnectionStringBuilder(context.Database.GetConnectionString());
        var resourceId = $"{builder.Host}_{builder.Port}_{builder.Database}";

        var myLock = new RedisLock(db, resourceId, expiry);

        while (true)
        {
            if (await myLock.AcquireWithRetryAsync(waitTimeout: wait, retryDelay: retry))
            {
                try
                {
                    await MigratePostgresIfNeedAsync(context);
                    return;
                }
                finally
                {
                    await myLock.ReleaseAsync();
                }
            }
            else
            {
                var pending = await context.Database.GetPendingMigrationsAsync();
                if (!pending.Any()) return;
            }

            await Task.Delay(retry);
        }
    }

    private static async Task MigratePostgresIfNeedAsync(DbContext context)
    {
        var connectionString = context.Database.GetConnectionString();

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"No connection string for db context");

        if (!await CheckDatabaseExistsAsync(connectionString))
            await CreateDatabaseAsync(connectionString);

        await context.Database.MigrateAsync();
    }

    private static async Task<bool> CheckDatabaseExistsAsync(string connectionString)
    {
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        var targetDb = builder.Database;

        if (string.IsNullOrEmpty(targetDb))
            throw new ArgumentException($"Not database in  {connectionString}", nameof(connectionString));

        builder.Database = "postgres";

        using var masterConn = new Npgsql.NpgsqlConnection(builder.ConnectionString);
        try
        {
            await masterConn.OpenAsync();

            using var checkCmd = new Npgsql.NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbName", masterConn);
            checkCmd.Parameters.AddWithValue("dbName", targetDb);

            return await checkCmd.ExecuteScalarAsync() != null;
        }
        finally
        {
            await masterConn.CloseAsync();
        }
    }

    private static async Task<string?> CreateDatabaseAsync(string connectionString)
    {
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        var targetDb = builder.Database;

        builder.Database = "postgres";

        using var masterConn = new Npgsql.NpgsqlConnection(builder.ConnectionString);       

        try
        {
            await masterConn.OpenAsync();
            using var createCmd = new Npgsql.NpgsqlCommand($"CREATE DATABASE \"{targetDb}\"", masterConn);
            await createCmd.ExecuteNonQueryAsync();

            return default;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P04" || ex.SqlState == "55P03")
        {
            return ex.SqlState;
        }
        finally
        {
            await masterConn.CloseAsync();
        }
    }

    public static async Task UseAlchemyPostgresqlMigrationWithRedisLockIfAvailableAsync<TContext>(this IHost app, string redisConnectionSection)
        where TContext : DbContext
    {
        var conf = app.Services.GetRequiredService<IConfiguration>();

        var redisConnectionString = conf?.GetConnectionString(redisConnectionSection);
        if (!string.IsNullOrEmpty(redisConnectionString))
            await app.UseAlchemyPostgresqlMigrationWithRedisLockAsync<TContext>(redisConnectionString);
        else
            await app.UsePostgresqlMigrationAsync<TContext>();
    }
}