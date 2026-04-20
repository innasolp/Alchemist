using DotNet.Testcontainers.Builders;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Test.PostresqlTestContainer;

public static class PostgresqlTestContainerHelper
{
    public static PostgreSqlContainer BuildPostgreSqlContainer(string host, int port = 5432, string user ="postgres", string password = "P@ssw0rd")
    {
        return new PostgreSqlBuilder("postgres:latest")
            .WithName(Guid.NewGuid().ToString("N"))
            .WithHostname(host)
           .WithPortBinding(port, true)
        .WithPassword(password)
        .WithUsername(user)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .Build();
    }

    public static string BuildConnectionString(this PostgreSqlContainer postgreSqlContainer, string dataBase, int publicPort = 5432, string user = "postgres", string password = "P@ssw0rd")
    {
        var sb = new NpgsqlConnectionStringBuilder
        {
            Host = postgreSqlContainer.Hostname,
            Port = postgreSqlContainer.GetMappedPublicPort(publicPort),
            Database = dataBase,
            Username = user,
            Password = password,
            KeepAlive = 30
        };

        return sb.ConnectionString;
    }
}