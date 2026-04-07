using Test.DbContainer.Abstractions;
using Testcontainers.PostgreSql;

namespace Test.PostresqlTestContainer;

public class PostgresqlTestDbContainer : ITestDbContainer
{
    private PostgreSqlContainer? _postgresqlContainer;

    public void Build(string host, int port = 5432, string password = "P@ssw0rd")
    {
        _postgresqlContainer = PostgresqlTestContainerHelper.BuildPostgreSqlContainer(host, port, password);
    }

    public string BuildConnectionString(string dataBase, int publicPort = 5432, string user = "postgres", string password = "P@ssw0rd")
    {
        if (_postgresqlContainer == null)
            throw new InvalidOperationException("PostgresqlTestContainer not built yet.");

        return _postgresqlContainer.BuildConnectionString(dataBase, publicPort, user, password);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_postgresqlContainer == null)
            throw new InvalidOperationException("PostgresqlTestContainer not built yet.");

        return _postgresqlContainer.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_postgresqlContainer != null)
            await _postgresqlContainer.StopAsync(cancellationToken);
    }
}