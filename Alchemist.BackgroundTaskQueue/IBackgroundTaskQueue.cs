using Microsoft.Extensions.Logging;

namespace BackgroundTaskQueue;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, ILogger, ValueTask> workItem, CancellationToken cancellationToken);

    ValueTask<Func<CancellationToken, ILogger, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken);
}