namespace Test.DbContainer.Abstractions;

public interface ITestDbContainer
{
    string BuildConnectionString(string dataBase, int publicPort, string user, string password);

    Task StartAsync(CancellationToken cancellationToken  = default);

    Task StopAsync(CancellationToken cancellationToken  = default);

    void Build(string host, int port, string password);
}
