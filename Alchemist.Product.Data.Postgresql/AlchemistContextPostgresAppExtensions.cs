using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Product.Data.Postgresql;

public static  class AlchemistContextPostgresAppExtensions
{
    private static readonly SemaphoreSlim _createDbSemaphoreSlim = new(1, 1);

    public static void UseAlchemyPostgresqlMigration(this IHost app)
    {
        using var scope = app.Services.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AlchemyContext>>();

        using var context = factory.CreateDbContext();

        context.Database.Migrate();
    }

    public static async Task UseAlchemyPostgresqlMigrationAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AlchemyContext>>();

        using var context = factory.CreateDbContext();

        var connectionString = context.Database.GetConnectionString();

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"No connection string for db context");

        try
        {
            await _createDbSemaphoreSlim.WaitAsync();

            if (!await CheckDatabaseExistsAsync(connectionString))
            {
                await CreateDatabaseAsync(connectionString);

                await context.Database.MigrateAsync();
            }
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "55P03")
        {}
        finally
        {
            ReleaseSemaphoreIfNeed(_createDbSemaphoreSlim);
        }
    }

    private static void ReleaseSemaphoreIfNeed(SemaphoreSlim semaphoreSlim)
    {
        if (semaphoreSlim.CurrentCount < 1)
        {
            semaphoreSlim.Release();
        }
    }

    private static async Task<bool> CheckDatabaseExistsAsync(string connectionString)
    {
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
        var targetDb = builder.Database;
        builder.Database = "postgres";

        using var masterConn = new Npgsql.NpgsqlConnection(builder.ConnectionString);
        try
        {
            await masterConn.OpenAsync();
            
            using var checkCmd = new Npgsql.NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{targetDb}'", masterConn);

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
            Console.WriteLine($"База {targetDb} создана.");

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
}