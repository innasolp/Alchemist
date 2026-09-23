using Respawn;
using System.Data.Common;

namespace Test.DbContainer.Abstractions;

public abstract class DatabaseRespawner : IDatabaseRespawner
{
    private Respawner? _respawner;

    private DbConnection? _dbConnection;

    protected abstract IDbAdapter DbAdapter { get; }

    public bool IsInitialized { get; private set; }

    public virtual async Task InitializeAsync(string dbconnectionString)
    {
        _dbConnection = GetDbConnection(dbconnectionString);

        if(_dbConnection.State != System.Data.ConnectionState.Open)
            await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, GetRespawnerOptions());

        IsInitialized = true;
    }

    protected virtual RespawnerOptions GetRespawnerOptions() => new()
    {
        DbAdapter = DbAdapter,
        SchemasToInclude = ["public"],
        TablesToIgnore =
            [
                "__EFMigrationsHistory" 
            ]
    };

    protected abstract DbConnection GetDbConnection(string connectionString);

    public Task ResetDatabaseAsync()
    {
        return _respawner != null && _dbConnection != null
            ? _respawner.ResetAsync(_dbConnection)
            : Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.DisposeAsync();
            _dbConnection = null;
        }
        GC.SuppressFinalize(this);
    }
}