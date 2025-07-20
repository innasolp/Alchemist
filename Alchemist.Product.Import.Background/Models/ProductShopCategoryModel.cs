using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Product.Import.Background.Models;

internal class ProductShopCategoryModel : IProductShopCategory
{
    public string Category { get ; set; }
    public int ItemId { get; set; }
}
