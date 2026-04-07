using Microsoft.EntityFrameworkCore;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.DBApiWebAppFactory.Configuration;

public abstract class DbConfigurationContainerWebAppFactory<TEntryPoint, TDbContext, TTestDbContainer> 
    : DbConfigurationWebAppFactory<TEntryPoint, TDbContext>, IAsyncLifetime
    where TEntryPoint : class
    where TDbContext : DbContext
    where TTestDbContainer : ITestDbContainer, new()
{
    private readonly TTestDbContainer _testDbContainer = new();

    private readonly string _host = Guid.NewGuid().ToString();

    protected override string ConnectionStringSection { get; }

    private string? _connectionString;

    private readonly string _database;

    private readonly string _user;
    private readonly string _password;
    private readonly int _port;

    protected override string ConnectionString => _connectionString ?? "";

    public DbConfigurationContainerWebAppFactory(string connectionStringSection, string database, int port, string user, string password)
    {
        ConnectionStringSection = connectionStringSection;
        _database = database;
        _user = user;
        _password = password;
        _port = port;

        _testDbContainer.Build(_host, port, password);
    }

    public async Task InitializeAsync()
    {
        await _testDbContainer.StartAsync();

        _connectionString = _testDbContainer.BuildConnectionString(_database, _port, _user, _password);
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _testDbContainer.StopAsync();
    }
}