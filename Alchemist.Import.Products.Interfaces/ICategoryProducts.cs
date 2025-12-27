namespace Alchemist.Import.Products.Interfaces;

public interface ICategoryProducts
{
    ICategoryProductItem[] CategoryProductItems { get; }

    int? TotalCount { get; }
}
