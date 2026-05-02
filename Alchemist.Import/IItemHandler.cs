namespace Alchemist.Import;

public class ItemProcessEventArgs<TItem, TStatus>(TItem item, TStatus processStatus, CancellationToken cancellationToken) : AsyncEventArgs(cancellationToken)
{
    public TStatus ProcessStatus { get; } = processStatus;

    public TItem Item { get; } = item;
}


public interface IItemHandler<TItem, TStatus>
{
    Task<TStatus> HandleItem(TItem item, CancellationToken cancellationToken = default);

    event AsyncEventHandler<ItemProcessEventArgs<TItem, TStatus>> ItemProcessed;
}