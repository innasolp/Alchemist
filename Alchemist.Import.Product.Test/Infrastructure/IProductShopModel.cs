using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.ProductService.Test.Infrastructure;

public interface IProductShopModel
{
    string ProductUrl { get; }
    string CategoryUrlFormat { get; }
    List<IProductShopCategory> Categories { get; }
    string Name { get; }
    string Url { get; }
}