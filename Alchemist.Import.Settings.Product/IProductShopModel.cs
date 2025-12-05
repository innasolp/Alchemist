using Alchemist.Import.Products.Interfaces;
using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Product;

public interface IProductShopModel : IImportSource
{
    string ProductUrl { get; set; }

    string CategoryUrl { get; set; }

    IEnumerable<IProductShopCategory> Categories { get; }
}
