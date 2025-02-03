using Alchemist.Common;

namespace Alchemist.Import.Shop.Interfaces;

public interface IItemHandler
{
    Task<ItemProcessStatus> HandleItem(object item, IShopModel shopModel);
}

public interface IItemHandler<T> : IItemHandler
{
    Task<ItemProcessStatus> HandleItem(T item, IShopModel shopModel);
}


