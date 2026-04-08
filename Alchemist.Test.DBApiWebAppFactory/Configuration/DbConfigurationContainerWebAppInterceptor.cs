using Alchemist.Test.Server.Fixtures;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.DBApiWebAppFactory.Configuration;

public abstract class DbConfigurationContainerWebAppInterceptor<TDbContext, TTestDbContainer, TDbRespawner>
    : DbConfigurationWebAppInterceptor<TDbContext>, IAsyncLifetime
    where TDbContext : DbContext
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
{
    private readonly TTestDbContainer _testDbContainer;

    private readonly TDbRespawner _dbRespawner;

    protected override string ConnectionStringSection { get; }

    private string? _connectionString;

    private readonly string _database;

    private readonly string _user;

    private readonly string _password;

    private readonly int _port;

    private readonly string _host = Guid.NewGuid().ToString();

    protected DbConfigurationContainerWebAppInterceptor(IWebHostConfigure webHostConfigure, 
        string connectionStringSection,
        string database,
        string user, 
        string password,
        int port, 
        TTestDbContainer? testDbContainer = null,
        TDbRespawner? dbRespawner = null) : base(webHostConfigure)
    {
        _testDbContainer = testDbContainer ?? new TTestDbContainer();

        _dbRespawner = dbRespawner ?? new TDbRespawner();

        ConnectionStringSection = connectionStringSection;
        _database = database;
        _user = user;
        _password = password;
        _port = port;

        _testDbContainer.Build(_host, port, password);
    }

    protected override string ConnectionString => _connectionString ?? "";

    public Task DisposeAsync()
    {
        Dispose();

        return _testDbContainer.StopAsync();
    }

    public async Task InitializeAsync()
    {
        await _testDbContainer.StartAsync();

        _connectionString = _testDbContainer.BuildConnectionString(_database, _port, _user, _password);

        var initializeConnectionString = _testDbContainer.BuildConnectionString("postgres", _port, _user, _password);

        await _dbRespawner.InitializeAsync(_database, _connectionString, initializeConnectionString);
    }

    public Task ResetDatabaseAsync()
    {
        return _dbRespawner.ResetDatabaseAsync();
    }
}