using Import.Interfaces;
using ShopImport.ServiceState;

namespace ShopImport.Product.Service.Stateful.Test.Infrastructure;

internal class TestServiceStateRepository : IServiceStateRepository
{
    private bool _isConnected = true;

    public bool IsConnected => _isConnected;

    public event AsyncEventHandler<ConnectedAsyncEventArgs> ConnectedAsync;

    private readonly Dictionary<string, object> _values = [];

    public Func<byte[], object>? LoadMock { get; set; }

    public Task Close(CancellationToken cancellationToken = default)
    {
        _isConnected = false;
        return Task.CompletedTask;
    }

    public Task Connect(CancellationToken cancellationToken = default)
    {
        _isConnected = true;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task<T?> Load<T>(byte[] key, CancellationToken cancellationToken = default)
    {
        if (LoadMock != null)
        {
            var callbackResult = LoadMock(key);
            return Task.FromResult(callbackResult is T t ? t : default);
        }

        string hexHash = Convert.ToHexString(key);

        if (_values.TryGetValue(hexHash, out var value))
        {
            return value is T result ? Task.FromResult(result) : Task.FromResult(default(T));
        }

        return Task.FromResult(default(T));
    }

    public Task Save<T>(byte[] key, T value, CancellationToken cancellationToken = default)
    {
        string hexHash = Convert.ToHexString(key);
        _values.TryAdd(hexHash, value);
        return Task.CompletedTask;
    }

    public Task Remove(byte[] key, CancellationToken cancellationToken = default)
    {
        string hexHash = Convert.ToHexString(key);
        _values.Remove(hexHash);
        return Task.CompletedTask;
    }
}