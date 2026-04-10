using Xunit;

namespace Test.DbContainer.Abstractions;

public interface ITestDbContainer : IAsyncLifetime
{
    string BuildConnectionString(string dataBase, int publicPort, string user, string password);

    void Build(string host, int port, string password);
}
