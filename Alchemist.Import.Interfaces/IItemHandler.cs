using Alchemist.Common;

namespace Alchemist.Import.Interfaces;

public interface IItemHandler
{
    Task<ResultStatus> HandleItem(IItem item, IShopModel shopModel);    
}

public interface IItemHandler<T> : IItemHandler
    where T : IItem
{
    Task<ResultStatus> HandleItem(T item, IShopModel shopModel);

    event AsyncItemHandler<T> ItemProcessed;
}


