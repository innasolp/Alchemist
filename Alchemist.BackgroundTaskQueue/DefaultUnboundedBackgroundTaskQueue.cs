using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Alchemist.BackgroundTaskQueue;

internal class DefaultUnboundedBackgroundTaskQueue(bool singleReader = false, bool singleWriter = false) : BackgroundTaskQueue
{
    private readonly bool _singleReader = singleReader;
    private readonly bool _singleWriter = singleWriter;

    protected override Channel<Func<CancellationToken, ILogger, ValueTask>> CreateCahnnel()
    {
        UnboundedChannelOptions options = new() { SingleReader = _singleReader, SingleWriter = _singleWriter };
        
        return Channel.CreateUnbounded<Func<CancellationToken, ILogger, ValueTask>>(options);
    }
}