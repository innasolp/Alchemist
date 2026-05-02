using Import.Interfaces;

namespace Alchemist.Import.Products.Interfaces;

public interface IImportProduct
{
    IProductItem ProductItem { get; }

    Stream Stream { get; }

    string SourceName { get; }

    string SourcePath { get; }
}

public interface IProductItemHandler : IItemHandler<IImportProduct, ResultStatus>
{
}
