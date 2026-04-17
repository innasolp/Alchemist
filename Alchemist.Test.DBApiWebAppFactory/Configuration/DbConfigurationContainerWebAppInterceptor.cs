using Alchemist.Test.Server.Fixtures;
using Microsoft.EntityFrameworkCore;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.DBApiWebAppFactory.Configuration;

public class DbConfigurationContainerWebAppInterceptor<TDbContext, TTestDbContainer, TDbRespawner, TDbHelper>
    : DbConfigurationWebAppInterceptor<TDbContext>, IAsyncLifetime
    where TDbContext : DbContext
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
    where TDbHelper : class, IDbHelper, new()
{
    private readonly TTestDbContainer _testDbContainer;

    private readonly TDbRespawner _dbRespawner;

    private readonly TDbHelper _dbHelper;

    protected override string ConnectionStringSection { get; }

    private string? _connectionString;

    private readonly string _database;

    private readonly string _user;

    private readonly string _password;

    private readonly int _port;

    private readonly string _host = Guid.NewGuid().ToString();

    private readonly Action<TDbContext>? _fillTestData;

    public DbConfigurationContainerWebAppInterceptor(IWebHostConfigure webHostConfigure, 
        string connectionStringSection,
        string database,
        string user, 
        string password,
        int port, 
        TTestDbContainer? testDbContainer = null,
        TDbRespawner? dbRespawner = null,
        TDbHelper? dbChecker = null,
        Action<TDbContext>? fillTestData = null) : base(webHostConfigure)
    {
        _testDbContainer = testDbContainer ?? new TTestDbContainer();
        _dbRespawner = dbRespawner ?? new TDbRespawner();
        _dbHelper = dbChecker ?? new TDbHelper();

        ConnectionStringSection = connectionStringSection;
        _database = database;
        _user = user;
        _password = password;
        _port = port;
        
        _fillTestData = fillTestData;

        _testDbContainer.Build(_host, port, user, password);
    }

    protected override string ConnectionString => _connectionString ?? "";

    protected override void FillTestData(TDbContext dbContext)
    {
        _fillTestData?.Invoke(dbContext);
    }

    public async Task DisposeAsync()
    {
        Dispose();

        await _testDbContainer.DisposeAsync();

        await _dbRespawner.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _testDbContainer.InitializeAsync();

        var initializeConnectionString = _testDbContainer.BuildConnectionString("postgres", _port);
        _connectionString = _testDbContainer.BuildConnectionString(_database, _port);

        if (!await _dbHelper.DatabaseExistsAsync(_database, initializeConnectionString))
            await _dbHelper.CreateDatabaseAsync(_database, initializeConnectionString);
        else 
            await _dbRespawner.InitializeAsync(_connectionString);
    }

    public async Task ResetDatabaseIfAvailableAsync()
    {  
        if (_dbRespawner.IsInitialized)
            await _dbRespawner.ResetDatabaseAsync();
    }
}