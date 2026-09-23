using Xunit;

namespace Test.DbContainer.Abstractions;

public class DbTestContainerFixture<TTestDbContainer> : IAsyncLifetime
    where TTestDbContainer : ITestDbContainer, new()
{
    public TTestDbContainer Container { get; } = new TTestDbContainer();

    public async Task InitializeAsync()
    {
        Container.Build(Guid.NewGuid().ToString());
        await Container.InitializeAsync();
    }

    public async Task DisposeAsync() => await Container.DisposeAsync();
}