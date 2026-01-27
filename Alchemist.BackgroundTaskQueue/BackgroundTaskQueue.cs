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

        while (true)
        {
            if (!await _queue.Writer.WaitToWriteAsync(cancellationToken).ConfigureAwait(false))
               throw new InvalidOperationException("The background task queue is closed and cannot accept new work items.");            

            if (_queue.Writer.TryWrite(workItem))            
                return;             
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

        throw new InvalidOperationException("The background task queue is completed and no more items can be read.");
    }
}