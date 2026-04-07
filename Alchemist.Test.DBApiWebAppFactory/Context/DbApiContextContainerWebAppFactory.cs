using Microsoft.EntityFrameworkCore;
using Test.DbContainer.Abstractions;

namespace Alchemist.Test.DBApiWebAppFactory.Context;

public abstract class DbApiContextContainerWebAppFactory<TEntryPoint, TDbContext, TTestDbContainer >
    : DbContextWebAppFactory<TEntryPoint, TDbContext>, IAsyncLifetime
    where TEntryPoint : class
    where TDbContext : DbContext
    where TTestDbContainer : ITestDbContainer, new()
{
    private readonly TTestDbContainer _testDbContainer = new();

    private readonly string _host = Guid.NewGuid().ToString();

    protected DbApiContextContainerWebAppFactory(int port, string password)
    {
        _testDbContainer.Build(_host, port, password);
    }

    public Task InitializeAsync()
    {
        return _testDbContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _testDbContainer.StopAsync();
    }
}