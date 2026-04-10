namespace Test.DbContainer.Abstractions;

public interface IDbChecker
{
    Task<bool> CheckDatabaseAsync(string database, string initializeConnectionString);
}