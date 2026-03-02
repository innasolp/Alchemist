using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Factory.Product.Json.Infrastructure;

public class Product : IProductItem
{
    public string Name { get; set; }
    public string ItemId { get; set; }
    public string Brand { get; set; }
    public string Currency { get; set; }
    public double Price { get; set; }
    public string Path { get; set; }
    public string AbsolutePath { get; set; }
    public int CategoryId { get; set; }
}