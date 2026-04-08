namespace Test.DbContainer.Abstractions;

public interface IDatabaseRespawner
{
    Task InitializeAsync(string database, string connectionString, string initializeConnectionString);

    Task ResetDatabaseAsync();
}