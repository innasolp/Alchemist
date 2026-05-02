namespace Alchemist.Common;

public delegate Task AsyncEventHandler<TAsyncEventArgs>(object sender, TAsyncEventArgs eventArgs)
    where TAsyncEventArgs : AsyncEventArgs;

public class AsyncEventArgs(CancellationToken cancellationToken) : EventArgs
{
    public CancellationToken CancellationToken { get; } = cancellationToken;
}
