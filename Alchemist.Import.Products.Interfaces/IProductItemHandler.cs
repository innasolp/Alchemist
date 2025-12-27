using Import.Interfaces;

namespace Alchemist.Import.Products.Interfaces;

public interface IImportProduct
{
    IProductItem ProductItem { get; }

    string SourceName { get; }

    string SourceUrl { get; }
}

public interface IProductItemHandler : IItemHandler<IImportProduct, ResultStatus>
{
}
