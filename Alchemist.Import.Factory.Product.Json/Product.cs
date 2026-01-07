using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Factory.Product.Json;

internal class Product : IProductItem
{
    public string Name { get; set; }
    public string ItemId { get; set; }
    public string Brand { get; set; }
    public string Currency { get; set; }
    public double Price { get; set; }
    public string Path { get; set; }
    public string ApiUrl { get; set; }
    public int CategoryId { get; set; }
}