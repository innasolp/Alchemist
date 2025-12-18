using Alchemist.Import.Products.Interfaces;
using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Product;

public interface IProductShopModel : IImportSource
{
    IEnumerable<IProductShopCategory> Categories { get; }
}