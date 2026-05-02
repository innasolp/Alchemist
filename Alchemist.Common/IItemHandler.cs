namespace Alchemist.Common;

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

public interface IItemHandler : IItemHandler<object, ItemProcessStatus>
{
    Type ItemType { get; }

    string EventName { get; }
}