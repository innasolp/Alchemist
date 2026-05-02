using StackExchange.Redis;

namespace Alchemist.Product.Data.Postgresql;

internal class RedisLock(IDatabase db, string resourceName, TimeSpan expiry)
{
    private readonly IDatabase _db = db;
    private readonly string _lockKey = $"lock:{resourceName}";
    private readonly string _lockValue = Guid.NewGuid().ToString();
    private readonly TimeSpan _expiry = expiry;

    public async Task<bool> AcquireAsync()
    {
        return await _db.StringSetAsync(_lockKey, _lockValue, _expiry, When.NotExists);
    }

    public async Task<bool> AcquireWithRetryAsync(TimeSpan waitTimeout, TimeSpan retryDelay)
    {
        var stopWatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopWatch.Elapsed < waitTimeout)
        {
            if (await _db.StringSetAsync(_lockKey, _lockValue, _expiry, When.NotExists))
            {
                return true;
            }

            await Task.Delay(retryDelay);
        }

        return false;
    }

    public async Task ReleaseAsync()
    {
        string script = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
        await _db.ScriptEvaluateAsync(script, [_lockKey], [_lockValue]);
    }
}