using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Products.Service;

public class CategoryResult<T>(T? category, int productCount, bool result)
    where T : class, ICategoryProducts
{
    public T? Category { get; } = category;

    public bool Result { get; } = result;

    public int ProductCount { get; } = productCount;
}
