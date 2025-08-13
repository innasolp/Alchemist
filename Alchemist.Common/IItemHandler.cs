namespace Alchemist.Common;

public interface IItemHandler<TItem, TStatus>
{
    Task<TStatus> HandleItem(TItem item);

    event AsyncItemHandler<TItem, TStatus> ItemProcessed;
}

public delegate Task AsyncItemHandler<TItem, TStatus>(object sender, TItem item, TStatus itemProcessStatus);