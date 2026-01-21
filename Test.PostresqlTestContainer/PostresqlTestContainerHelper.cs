using DotNet.Testcontainers.Builders;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Test.PostresqlTestContainer;

public static class PostresqlTestContainerHelper
{
    public static PostgreSqlContainer BuildPostgreSqlContainer(string host, int port = 5432, string password = "P@ssw0rd")
    {
        return new PostgreSqlBuilder("postgres:latest")
            .WithName(Guid.NewGuid().ToString("N"))
            .WithHostname(host)
            .WithExposedPort(port)
        .WithPortBinding(port, true)
        .WithEnvironment("POSTGRES_PASSWORD", password)
        .WithEnvironment("PGDATA", "/pgdata")
        .WithTmpfsMount("/pgdata")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("psql -U postgres -c \"select 1\""))
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
            Password = password
        };

        return sb.ConnectionString;
    }
}