using Xunit;

namespace Test.DbContainer.Abstractions;

public interface ITestDbContainer : IAsyncLifetime
{
    string BuildConnectionString(string dataBase, int port);

    void Build(string host, int port = 5432, string user = "postgres", string password = "P@ssw0rd");
}