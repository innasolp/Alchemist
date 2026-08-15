using DotNet.Testcontainers.Containers;
using Xunit;

namespace Test.DbContainer.Abstractions;

public abstract class TestDbContainer : ITestDbContainer, IAsyncDisposable
{
    protected abstract DockerContainer? DockerContainer { get; }

    public abstract  void Build(string host, int port, string user, string password);

    public abstract string BuildConnectionString(string dataBase, int port);

    Task IAsyncLifetime.DisposeAsync()
    {
        return StopAsync();
    }

    protected virtual async Task StopAsync()
    {
        if (DockerContainer != null)
            await DockerContainer.StopAsync();
    }

    public async Task InitializeAsync()
    {
        if (DockerContainer == null)
            throw new InvalidOperationException($"{GetType().Name} not built yet.");

        if (DockerContainer.State !=TestcontainersStates.Running)
            await DockerContainer.StartAsync();
    }

    ValueTask IAsyncDisposable.DisposeAsync() => DisposeAsync();

    protected virtual async ValueTask DisposeAsync()
    {
        if (DockerContainer != null)
            await DockerContainer.DisposeAsync();
    }
}
