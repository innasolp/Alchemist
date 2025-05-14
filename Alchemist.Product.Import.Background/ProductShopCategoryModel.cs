using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Product.Import.Background;

internal class ProductShopCategoryModel : IProductShopCategoryModel
{
    public string Category { get ; set; }
    public int ItemId { get; set; }
}
