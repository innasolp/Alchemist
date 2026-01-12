using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client;

public interface IRateLimiterWebLoader : IAsyncDisposable
{
    bool IsStarted(Guid connectionId);

    bool IsConnected(Guid connectionId);


    event AsyncEventHandler<EventArgs>? Reseted;

    Task AddToPool(Guid connectionId, CancellationToken cancellationToken = default);

    Task Close(Guid connectionId, CancellationToken cancellationToken = default);

    Task Reset(string host);

    Task<bool> Start(Guid connectionId);

    Task<T> ExecuteAsync<T>(Guid connectionId, Func<IWebLoader, CancellationToken, Task<T>> task, CancellationToken cancellationToken = default);
}