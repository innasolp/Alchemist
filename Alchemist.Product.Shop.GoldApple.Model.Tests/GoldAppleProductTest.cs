using Alchemist.Import.Products.Interfaces;
using System.Text.Json;

namespace Alchemist.Product.Shop.GoldApple.Model.Tests;

public class GoldAppleProductTest
{
    private readonly ProductData? _product;
    public GoldAppleProductTest()
    {
        using var stream = File.OpenRead("GoldAppleProduct.json");
        _product = JsonSerializer.Deserialize<ProductData>(stream);
        stream.Close();
    }

    [Fact]
    public void ProductJsonDeserializationSuccess()
    {
        Assert.NotNull(_product);
        Assert.NotNull(_product.Data);
    }

    [Fact]
    public void ProductItemIsValid()
    {
        var productItem = _product as IProductItem;
        Assert.NotNull(productItem.Purposes);
        Assert.NotNull(productItem.Country);
        Assert.NotNull(productItem.Brand);
        Assert.NotNull(productItem.ProductType);
    }
}
