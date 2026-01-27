using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace BackgroundTaskQueue;

internal abstract class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, ILogger, ValueTask>> _queue;

    public BackgroundTaskQueue()
    {
        _queue = CreateChannel();
    }

    protected abstract Channel<Func<CancellationToken, ILogger, ValueTask>> CreateChannel();

    public async ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, ILogger, ValueTask> workItem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        while (await _queue.Writer.WaitToWriteAsync(cancellationToken).ConfigureAwait(false))
        {
            if (_queue.Writer.TryWrite(workItem))
            {
                return; // Successfully wrote the item
            }
            // Another producer might have written to the channel between the await
            // and the TryWrite call. The loop handles this by waiting again.
        }
    }

    public async ValueTask<Func<CancellationToken, ILogger, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        if (await _queue.Reader.WaitToReadAsync(cancellationToken))
        {
            Func<CancellationToken, ILogger, ValueTask>? workItem =
                await _queue.Reader.ReadAsync(cancellationToken);

            return workItem;
        }
        throw new InvalidOperationException($"can't read from queue");
    }
}