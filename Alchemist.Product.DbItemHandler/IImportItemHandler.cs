using Alchemist.Common;

namespace Alchemist.Product.DbItemHandler;

public interface IImportItemHandler : IItemHandler<object, ItemProcessStatus>
{
    Type ItemType { get; }

    string EventName { get; }
}
