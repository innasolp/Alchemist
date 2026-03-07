using Import.LoaderSettings;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
using System.Threading.RateLimiting;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client;

public class RatelimiterWebLoader(IWebLoader webLoader, RateLimiterOptions? rateLimiterOptions = null) : IRateLimiterWebLoader
{
    private readonly RateLimiterOptions _rateLimiterOptions = rateLimiterOptions ?? new RateLimiterOptions
    {
        WindowMilliseconds = DefaultLimiterWindowInMilliseconds,
        QueueLimit = DefaultQueueLimit
    };

    private readonly HashSet<Guid> _connectionsPool = [];

    private readonly IWebLoader _webLoader = webLoader;

    private readonly SemaphoreSlim _addToPoolSemaphore = new(1,1);
    private readonly SemaphoreSlim _removeFromPoolSemaphore = new(1,1);
    private readonly SemaphoreSlim _startSemaphoreSlim = new(1,1);
    
    private ResiliencePipeline? _pipeline;

    private const int DefaultLimiterWindowInMilliseconds = 2000;

    private const int DefaultQueueLimit = 5;

    private void ThrowExceptionIfNotStarted(Guid connectionId)
    {
        if (!IsStarted(connectionId))
            throw new Exception($"Web loader for connection {connectionId} not started");
    }

    private void Initialize()
    {
        var limiter = new FixedWindowRateLimiter(
            new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMilliseconds(_rateLimiterOptions.WindowMilliseconds ?? DefaultLimiterWindowInMilliseconds),
                PermitLimit = 1,
                QueueLimit = _rateLimiterOptions.QueueLimit ?? DefaultQueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });

        var pipelineBuilder = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<RateLimiterRejectedException>(),
            // Use the RetryAfter value from the exception to determine the delay
            DelayGenerator = args =>
            {
                if (args.Outcome.Exception is RateLimiterRejectedException rateLimiterRejectedException)
                {
                    // Wait for the requested duration before retrying
                    return ValueTask.FromResult(rateLimiterRejectedException.RetryAfter);
                }
                return ValueTask.FromResult<TimeSpan?>(TimeSpan.Zero);
            },
            BackoffType = DelayBackoffType.Constant,
            Delay = TimeSpan.FromSeconds(1),
            MaxRetryAttempts = 5
        })
            .AddRateLimiter(limiter);

        _pipeline = pipelineBuilder.Build();
    }

    public void UpdateLimiterOptionsIfNeed(RateLimiterOptions rateLimiterOptions)
    {
        if(rateLimiterOptions.WindowMilliseconds.HasValue)
            _rateLimiterOptions.WindowMilliseconds = Math.Max(_rateLimiterOptions.WindowMilliseconds!.Value, rateLimiterOptions.WindowMilliseconds.Value);
        
        if(rateLimiterOptions.QueueLimit.HasValue)
            _rateLimiterOptions.QueueLimit = Math.Max(_rateLimiterOptions.QueueLimit!.Value, rateLimiterOptions.QueueLimit.Value);
    }

    public bool IsStarted(Guid connectionId)
    {
        return _connectionsPool.Contains(connectionId) && _webLoader.IsStarted;
    }

    public event AsyncEventHandler<EventArgs>? Reseted;

    public async Task AddToPool(Guid connectionId, CancellationToken cancellationToken = default)
    {
        await _addToPoolSemaphore.WaitAsync(cancellationToken);

        try
        {
            if (_pipeline == null) Initialize();

            _connectionsPool.Add(connectionId);
        }
        finally
        {
            _addToPoolSemaphore.Release();
        }
    }

    public async Task Close(Guid connectionId, CancellationToken cancellationToken  = default)
    {
        await _removeFromPoolSemaphore.WaitAsync(cancellationToken);

        try
        {
            _connectionsPool.Remove(connectionId);

            if (_connectionsPool.Count == 0 && _webLoader.IsStarted)
                await _webLoader.Close();
        }
        finally
        {
            _removeFromPoolSemaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _connectionsPool.Clear();

        try
        {
            if (_webLoader.IsStarted)
                await _webLoader.Close();
        }
        finally
        {
            _addToPoolSemaphore.Dispose();
            _startSemaphoreSlim.Dispose();
            _removeFromPoolSemaphore.Dispose();
        }

        await _webLoader.DisposeAsync();
    }

    public async Task Reset(string host)
    {
        await _webLoader.Reset(host);

        if(Reseted != null)
            await Reseted.Invoke(this, new EventArgs());
    }

    public async Task<bool> Start(Guid connectionId, CancellationToken cancellationToken = default)
    {
        if (!_connectionsPool.Contains(connectionId)) return false;

        await _startSemaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            if (!_webLoader.IsStarted) return await _webLoader.Start();
        }
        finally
        {
            _startSemaphoreSlim.Release();
        }

        return true;
    }

    public async Task<T> ExecuteAsync<T>(Guid connectionId, Func<IWebLoader,CancellationToken, Task<T>> task, CancellationToken cancellationToken = default)
    {
        ThrowExceptionIfNotStarted(connectionId);

        ValueTask<T> valueTask(CancellationToken ct) => new(task(_webLoader, ct));
        return await _pipeline.ExecuteAsync(valueTask, cancellationToken);
    }

    public bool IsConnected(Guid connectionId)
    {
        return _connectionsPool.Contains(connectionId);
    }
}