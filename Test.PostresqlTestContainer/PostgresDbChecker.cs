using Npgsql;
using Test.DbContainer.Abstractions;

namespace Test.PostresqlTestContainer;

public class PostgresDbChecker : IDbChecker
{
    public async Task<bool> CheckDatabaseAsync(string database, string initializeConnectionString)
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