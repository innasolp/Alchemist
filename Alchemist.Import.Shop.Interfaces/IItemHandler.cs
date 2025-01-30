using Alchemist.Common;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Shop.Interfaces;

public interface IItemHandler
{
    Task<ItemProcessStatus> HandleItem(object item, IShopUrlModel shopUrlModel);
}

public interface IItemHandler<T> : IItemHandler
{
    Task<ItemProcessStatus> HandleItem(T item, IShopUrlModel shopUrlModel);
}


