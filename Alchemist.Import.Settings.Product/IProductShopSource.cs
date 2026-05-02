using Alchemist.Import.Products.Interfaces;
using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Product;

public interface IProductShopSource : IImportSource
{
    IEnumerable<IProductShopCategory> Categories { get; }
}