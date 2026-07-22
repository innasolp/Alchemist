using Npgsql;
using Test.DbContainer.Abstractions;
using Testcontainers.PostgreSql;
using Xunit;

namespace Test.PostresqlTestContainer;

public class PostgresqlTestDbContainer : ITestDbContainer
{
    private PostgreSqlContainer? _postgresqlContainer;

    public void Build(string host, int port=5432, string user = "postgres", string password = "P@ssw0rd")
    {
        _postgresqlContainer = PostgresqlTestContainerHelper.BuildPostgreSqlContainer(host, port, user, password);
    }

    public string BuildConnectionString(string dataBase, int port)
    {
        if (_postgresqlContainer == null)
            throw new InvalidOperationException("PostgresqlTestContainer not built yet.");

        var connectionString = _postgresqlContainer.GetConnectionString();
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        var user = builder.Username;
        var password = builder.Password;

var connectionString = _postgresqlContainer.BuildConnectionString(dataBase, port, user, password);
return connectionString.EndsWith(';') ? connectionString + "Pooling=false;" : connectionString + ";Pooling=false;";
    }

    public Task InitializeAsync()
    {
        if (_postgresqlContainer == null)
            throw new InvalidOperationException("PostgresqlTestContainer not built yet.");

        return _postgresqlContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();

        if (_postgresqlContainer != null)
        {
            await _postgresqlContainer.StopAsync();
            await _postgresqlContainer.DisposeAsync();
        }       
    }
}