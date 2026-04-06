using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System.Data.Common;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;
using Xunit;

namespace ShopImport.Test.DbApiWebAppFactory.Postgresql;

public abstract class DbApiConfigurationPostgresWebAppFactory<TEntryPoint, TDbContext> : DbConfigurationWebAppFactory<TEntryPoint, TDbContext>, IAsyncLifetime
    where TEntryPoint : class
    where TDbContext : DbContext
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    private Respawner? _respawner;

    private DbConnection? _connection;


    private readonly string _host = Guid.NewGuid().ToString();

    protected override string ConnectionStringSection { get; }

    protected override string ConnectionString { get; }

    public DbApiConfigurationPostgresWebAppFactory(string connectionStringSection, string database)
    {
        ConnectionStringSection = connectionStringSection;

        _postgreSqlContainer = PostresqlTestContainerHelper.BuildPostgreSqlContainer(_host);

        ConnectionString = _postgreSqlContainer.BuildConnectionString(database);
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var host = Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = host.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        _connection = dbContext.Database.GetDbConnection();
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if(_respawner != null && _connection != null)
            await _respawner.ResetAsync(_connection);
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return _postgreSqlContainer.StopAsync();
    }
}