using Alchemist.Import.Products.Interfaces;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.GoldApple.Model;

public class CategoryProducts: ICategoryProducts
{
    [JsonPropertyName("data")]
    public ProductCategoryData Data { get; set; }

    ICategoryProductItem[] ICategoryProducts.CategoryProductItems => [.. Data.Products.Select(c=>c.Product)];

    int? ICategoryProducts.TotalCount => Data.Count;
}
