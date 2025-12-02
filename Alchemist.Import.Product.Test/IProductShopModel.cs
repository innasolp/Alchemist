using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Product.Test;

public interface IProductShopModel
{
    string ProductUrl { get; }
    string CategoryUrl { get; }
    List<IProductShopCategory> Categories { get; }
    string Name { get; }
    string Url { get; }
}