using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace BackgroundTaskQueue;

public sealed class BoundedBackgroundTaskQueue(int capacity) : BackgroundTaskQueue
{
    private readonly int _capacity = capacity;

    protected override Channel<Func<CancellationToken, ILogger, ValueTask>> CreateChannel()
    {
        BoundedChannelOptions options = new(_capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        return Channel.CreateBounded<Func<CancellationToken, ILogger, ValueTask>>(options);
    }
}