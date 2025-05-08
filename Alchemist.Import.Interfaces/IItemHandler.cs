using Alchemist.Common;

namespace Alchemist.Import.Interfaces;

public interface IItemHandler
{
    Task<ItemProcessStatus> HandleItem(object item, int shopId);
}

public interface IItemHandler<T> : IItemHandler
{
    Task<ItemProcessStatus> HandleItem(T item, int shopId);
}


