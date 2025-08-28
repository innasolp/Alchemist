namespace Alchemist.Import.Interfaces;

public interface ILoaderService : IAsyncDisposable
{
    string Name { get; }

    Task Start();

    Task Reset();

    Task Close();

    bool IsStarted { get; }

    Task<object> GetData(string host);

    Task UpdateData(string url);

    Task<Stream> Load(string url, object? data);
}
