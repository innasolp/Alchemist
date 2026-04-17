using Xunit;

namespace Test.DbContainer.Abstractions;

public interface ITestDbContainer : IAsyncLifetime
{
    string BuildConnectionString(string dataBase, int port);

    void Build(string host, int port, string user, string password);
}
