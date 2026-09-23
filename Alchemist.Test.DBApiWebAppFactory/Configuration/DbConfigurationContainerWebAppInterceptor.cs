using Alchemist.Test.Server.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.DBApiWebAppFactory.Configuration;

public class DbConfigurationContainerWebAppInterceptor<TDbContext, TTestDbContainer, TDbRespawner, TDbHelper>
    : DbConfigurationWebAppInterceptor<TDbContext>, IAsyncLifetime
    where TDbContext : DbContext
    where TTestDbContainer : class, ITestDbContainer, new()
    where TDbRespawner : class, IDatabaseRespawner, new()
    where TDbHelper : class, IDbHelper, new()
{
    private readonly TTestDbContainer? _localTestDbContainer = null;

    private readonly TTestDbContainer? _testDbContainer = null;

    private readonly TDbRespawner _dbRespawner;

    private readonly TDbHelper _dbHelper;

    protected override string ConnectionStringSection { get; }

    private TTestDbContainer? GetTestDbContainer()
    {
        return _localTestDbContainer ?? _testDbContainer;
    }

    private string? _connectionString;
    private readonly IServices _services;
    private readonly string _database;

    private readonly string _user;

    private readonly string _password;

    private readonly int _port;

    private readonly string _host = Guid.NewGuid().ToString();

    private readonly Action<TDbContext>? _fillTestData;

    public DbConfigurationContainerWebAppInterceptor(IServices services, 
        IWebHostConfigure webHostConfigure, 
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
        _dbRespawner = dbRespawner ?? new TDbRespawner();
        _dbHelper = dbChecker ?? new TDbHelper();
        _services = services;
        ConnectionStringSection = connectionStringSection;
        _database = database;
        _user = user;
        _password = password;
        _port = port;
        
        _fillTestData = fillTestData;

        if (testDbContainer == null)
        {
            _localTestDbContainer = new TTestDbContainer();
            _localTestDbContainer.Build(_host, port, user, password);
        }
        else 
            _testDbContainer = testDbContainer;
    }

    protected override string ConnectionString => _connectionString ?? "";

    protected override void FillTestData(TDbContext dbContext)
    {
        _fillTestData?.Invoke(dbContext);
    }

    public async Task DisposeAsync()
    {
        Dispose();

        if(_localTestDbContainer != null)
            await _localTestDbContainer.DisposeAsync();

        await _dbRespawner.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        if (_localTestDbContainer != null)
            await _localTestDbContainer.InitializeAsync();

        var initializeConnectionString = GetTestDbContainer()!.BuildConnectionString("postgres", _port);
        _connectionString = GetTestDbContainer()!.BuildConnectionString(_database, _port);

        if (!await _dbHelper.DatabaseExistsAsync(_database, initializeConnectionString))
        {
            await _dbHelper.CreateDatabaseAsync(_database, initializeConnectionString);

            using var scope = _services.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            await dbContext.Database.MigrateAsync();
        }
        else
            await _dbRespawner.InitializeAsync(_connectionString);
    }

    public async Task ResetDatabaseIfAvailableAsync()
    {  
        if (_dbRespawner.IsInitialized)
            await _dbRespawner.ResetDatabaseAsync();
    }
}