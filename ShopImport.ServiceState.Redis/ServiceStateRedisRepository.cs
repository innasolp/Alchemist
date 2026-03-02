using Import.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace ShopImport.ServiceState.Redis;

public class ServiceStateRedisRepository(string connectionString) : IServiceStateRepository
{
    public bool IsConnected => _redis?.IsConnected == true && _database != null;

    public event AsyncEventHandler<ConnectedAsyncEventArgs> ConnectedAsync;

    private ConnectionMultiplexer? _redis;

    private IDatabase? _database;

    public async Task Close(CancellationToken cancellationToken = default)
    {
        if (_redis != null)
        {
            try
            {
                await _redis.CloseAsync();
                await InvokeConnectedAsync(false, null, cancellationToken);
            }
            catch (Exception e)
            {
                await InvokeConnectedAsync(_redis.IsConnected, e, cancellationToken);
                throw;
            }
        }
    }

    public async Task Connect(CancellationToken cancellationToken = default)
    {
        try
        {
            _redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
            await InvokeConnectedAsync(_redis.IsConnected, null, cancellationToken);
        }
        catch(Exception e)
        {
            await InvokeConnectedAsync(_redis?.IsConnected == true, e, cancellationToken);
            throw;
        }

        _database = _redis?.GetDatabase();
    }

    protected async Task InvokeConnectedAsync(bool success, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        // Safe event invocation: capture and iterate to isolate handler failures
        var handlers = ConnectedAsync;
        if (handlers == null)
            return;

        var args = new ConnectedAsyncEventArgs(success, exception, cancellationToken);

        async Task HandlerTask(AsyncEventHandler<ConnectedAsyncEventArgs> handler)
        {
            await handler(this, args).ConfigureAwait(false);
        };

        var tasks = handlers.GetInvocationList()
            .OfType<AsyncEventHandler<ConnectedAsyncEventArgs>>()
            .Select(HandlerTask);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await Close();

        if (_redis != null)
            await _redis.DisposeAsync();
    }

    public async Task<T?> Load<T>(byte[] key, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException($"Database {connectionString} not connected.");

        var keyExists = await _database!.KeyExistsAsync(key);
        if (!keyExists)
            return default;

        var json = await _database.StringGetAsync(key);
        return JsonSerializer.Deserialize<T>(json);
    }

    public Task Save<T>(byte[] key, T value, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException($"Database {connectionString} not connected.");

        var json = JsonSerializer.Serialize(value);
        return _database!.StringSetAsync(key, json);
    }
}