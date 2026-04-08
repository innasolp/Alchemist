using Respawn;
using System.Data.Common;

namespace Test.DbContainer.Abstractions;

public abstract class DatabaseRespawner : IDatabaseRespawner
{
    private Respawner? _respawner;

    private DbConnection? _dbConnection;

    protected abstract IDbAdapter DbAdapter { get; }

    public virtual async Task InitializeAsync(string database, string dbconnectionString, string initializeConnectionString)
    {
        if(!await CheckDatabaseAsync(database, initializeConnectionString)) return;

        _dbConnection = GetDbConnection(dbconnectionString);

        if(_dbConnection.State != System.Data.ConnectionState.Open)
            await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter,
            SchemasToInclude = new[] { "public" }
        });
    }

    protected abstract Task<bool> CheckDatabaseAsync(string database, string initializeConnectionString);

    protected abstract DbConnection GetDbConnection(string connectionString);

    public Task ResetDatabaseAsync()
    {
        return _respawner != null && _dbConnection != null
            ? _respawner.ResetAsync(_dbConnection)
            : Task.CompletedTask;
    }
}