using Npgsql;
using Respawn;
using System.Data.Common;
using Test.DbContainer.Abstractions;

namespace Test.PostresqlTestContainer;

public class PostgresDbRespawner : DatabaseRespawner
{
    protected override IDbAdapter DbAdapter => Respawn.DbAdapter.Postgres;

    protected override DbConnection GetDbConnection(string connectionString)
    {
        return new NpgsqlConnection(connectionString);
    }

    protected override async Task<bool> CheckDatabaseAsync(string database, string initializeConnectionString)
    {
        await using var connection = new NpgsqlConnection(initializeConnectionString);

        await connection.OpenAsync();

        using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{database}'";
        var result = await checkCommand.ExecuteScalarAsync();

        await connection.CloseAsync();

        return result != null;
    }
}