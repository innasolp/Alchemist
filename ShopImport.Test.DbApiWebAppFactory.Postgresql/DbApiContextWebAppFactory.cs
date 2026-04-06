using Alchemist.Test.DBApiWebAppFactory.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.PostresqlTestContainer;
using Testcontainers.PostgreSql;
using Xunit;

namespace ShopImport.Test.DbApiWebAppFactory.Postgresql;

public abstract class DbApiContextWebAppFactory<TEntryPoint, TDbContext> : DbContextWebAppFactory<TEntryPoint, TDbContext>, IAsyncLifetime
    where TEntryPoint : class
    where TDbContext : DbContext
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    private readonly string _host = Guid.NewGuid().ToString();

    protected DbApiContextWebAppFactory()
    {
        _postgreSqlContainer = PostresqlTestContainerHelper.BuildPostgreSqlContainer(_host);
    }

    protected abstract IServiceCollection AddDbContext(IServiceCollection services,  Func<DbContextOptionsBuilder, DbContextOptionsBuilder> buildContextAction);

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return AddDbContext(services, optionsBuilder => optionsBuilder.UseNpgsql(_postgreSqlContainer.BuildConnectionString(DataBase)));
    }

    public Task InitializeAsync()
    {
        return _postgreSqlContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}