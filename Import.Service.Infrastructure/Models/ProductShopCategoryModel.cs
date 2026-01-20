using Alchemist.Import.Products.Interfaces;

namespace Import.Service.Commands.Models;

internal class ProductShopCategoryModel : IProductShopCategory
{
    public string Category { get ; set; }

    public int ItemId { get; set; }

    public string Path { get; set; }
}