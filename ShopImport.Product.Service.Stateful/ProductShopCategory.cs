using Alchemist.Import.Products.Interfaces;
namespace ShopImport.Product.Service.Stateful;

internal class ProductShopCategory : IProductShopCategory
{
    public string Category { get; set; }

    public int ItemId { get; set; }

    public string Path { get; set; }
}
