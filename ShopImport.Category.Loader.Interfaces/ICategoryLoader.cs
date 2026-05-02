using Alchemist.Import.Category.Interfaces;

namespace ShopImport.Category.Loader.Interfaces;

public interface ICategoryLoader
{
    ICategoryLoadOptions CategoryLoadOptions { get; }

    Task<IEnumerable<ICategory>> LoadAsync(ICategory? parentCategory, Stream stream, CancellationToken cancellationToken = default);

    public bool IsRecursive { get; }
}