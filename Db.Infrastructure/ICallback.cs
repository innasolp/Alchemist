namespace Db.Infrastructure;

public class AsyncEventArgs(Exception? exception = null, CancellationToken cancellationToken = default) : EventArgs
{
    public Exception? Exception { get; } = exception;

    public CancellationToken CancellationToken { get; } = cancellationToken;
}

public class CallbackAsyncEventArgs<T>(T value, Exception? exception = null, CancellationToken cancellationToken = default) 
    : AsyncEventArgs(exception, cancellationToken)
{
    public T Value { get; } = value;
}

public delegate Task AsyncEventHandler<TArgs>(object sender, TArgs args)
    where TArgs : AsyncEventArgs;

public interface ICallback<T>    
{
    event AsyncEventHandler<CallbackAsyncEventArgs<T>> Callback; 
}