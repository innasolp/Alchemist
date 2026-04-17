namespace Test.DbContainer.Abstractions;

public interface IDbHelper
{
    Task<bool> DatabaseExistsAsync(string database, string initializeConnectionString);

    Task CreateDatabaseAsync(string database, string initialConnectionString);
}