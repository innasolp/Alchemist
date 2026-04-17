using Npgsql;
using Test.DbContainer.Abstractions;

namespace Test.PostresqlTestContainer;

public class PostgresDbHelper : IDbHelper
{
    public async Task CreateDatabaseAsync(string database, string masterConnectionString)
    {
        using var connection = new NpgsqlConnection(masterConnectionString);
        await connection.OpenAsync();
        using var command = new NpgsqlCommand($"CREATE DATABASE {database};", connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> DatabaseExistsAsync(string database, string initializeConnectionString)
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