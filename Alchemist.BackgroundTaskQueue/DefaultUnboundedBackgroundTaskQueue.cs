using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace BackgroundTaskQueue;

internal class DefaultUnboundedBackgroundTaskQueue(bool singleReader = false, bool singleWriter = false) : BackgroundTaskQueue
{
    private readonly bool _singleReader = singleReader;
    private readonly bool _singleWriter = singleWriter;

    protected override Channel<Func<CancellationToken, ILogger, ValueTask>> CreateChannel()
    {
        UnboundedChannelOptions options = new() { SingleReader = _singleReader, SingleWriter = _singleWriter };
        
        return Channel.CreateUnbounded<Func<CancellationToken, ILogger, ValueTask>>(options);
    }
}