using Alchemist.Common;

namespace Alchemist.Product.ImportItem.Handler;

public interface IItemHandler
{
    Task<ItemProcessStatus> HandleItem(object item);
}

public interface IItemHandler<T>: IItemHandler
{
    Task<ItemProcessStatus> HandleItem(T item);
}
