using Alchemist.Common;
using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Products.Interfaces;

public interface IImportProduct
{
    IProductItem ProductItem { get; }

    IShopItem Shop { get; }
}

public interface IProductItemHandler : IItemHandler<IImportProduct, ResultStatus>
{
}
