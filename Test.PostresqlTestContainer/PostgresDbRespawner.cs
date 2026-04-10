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
}