using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Alchemist.BackgroundTaskQueue;

internal abstract class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, ILogger, ValueTask>> _queue;

    public BackgroundTaskQueue()
    {
        _queue = CreateCahnnel();
    }

    protected abstract Channel<Func<CancellationToken, ILogger, ValueTask>> CreateCahnnel();

    public async ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, ILogger, ValueTask> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<CancellationToken, ILogger, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        Func<CancellationToken, ILogger, ValueTask>? workItem =
            await _queue.Reader.ReadAsync(cancellationToken);

        return workItem;
    }
}