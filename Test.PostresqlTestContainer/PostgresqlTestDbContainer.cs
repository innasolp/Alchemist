using DotNet.Testcontainers.Containers;
using Npgsql;
using Test.DbContainer.Abstractions;
using Testcontainers.PostgreSql;

namespace Test.PostresqlTestContainer;

public class PostgresqlTestDbContainer : TestDbContainer
{
    private PostgreSqlContainer? _postgresqlContainer;

    protected override DockerContainer? DockerContainer => _postgresqlContainer;

    public override void Build(string host, int port=5432, string user = "postgres", string password = "P@ssw0rd")
    {
        _postgresqlContainer ??= PostgresqlTestContainerHelper.BuildPostgreSqlContainer(host, port, user, password);
    }

    public override string BuildConnectionString(string dataBase, int port)
    {
        if (_postgresqlContainer == null)
            throw new InvalidOperationException("PostgresqlTestContainer not built yet.");

        var connectionString = _postgresqlContainer.GetConnectionString();
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        var user = builder.Username;
        var password = builder.Password;

        var resultConnectionString = _postgresqlContainer.BuildConnectionString(dataBase, port, user, password);
        return resultConnectionString.EndsWith(';') ? resultConnectionString + "Pooling=false;" : resultConnectionString + ";Pooling=false;";
    }

    protected override async Task StopAsync()
    {
        NpgsqlConnection.ClearAllPools();

        await base.StopAsync();
    }
}