namespace Test.DbContainer.Abstractions;

public interface IDatabaseRespawner : IAsyncDisposable
{
    Task InitializeAsync(string connectionString);

    Task ResetDatabaseAsync();

    bool IsInitialized { get; }
}