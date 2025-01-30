using Alchemist.Import.Products.Interfaces;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Shop.GoldApple.Model;

public class CategoryProducts: ICategoryProducts
{
    [JsonPropertyName("data")]
    public ProductCategoryData Data { get; set; }

    ICategoryProductItem[] ICategoryProducts.CategoryProductItems => Data.Products;

    int ICategoryProducts.TotalCount => Data.Count;
}
