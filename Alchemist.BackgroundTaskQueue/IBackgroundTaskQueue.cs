using Microsoft.Extensions.Logging;

namespace Alchemist.BackgroundTaskQueue;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, ILogger, ValueTask> workItem);

    ValueTask<Func<CancellationToken, ILogger, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken);
}