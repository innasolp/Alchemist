using Alchemist.Common;

namespace Alchemist.Product.ImportItem.Handler;

public interface IImportItemHandler : IItemHandler<object, ItemProcessStatus>
{
    Type ItemType { get; }

    string EventName { get; }
}
