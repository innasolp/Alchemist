using Alchemist.Import.Products.Interfaces;
using System.Text.Json;

namespace Alchemist.Product.Shop.Ozon.Model.Tests;

public class OzonProductTest
{
    private readonly Product? _product;
    public OzonProductTest()
    {
        using var stream = File.OpenRead("OzonProduct.json");
        _product = JsonSerializer.Deserialize<Product>(stream);
        stream.Close();
    }

    [Fact]
    public void ProductJsonDeserializationSuccess()
    {        
        Assert.NotNull(_product);
        Assert.NotNull(_product.WebCharacteristics);
        Assert.NotNull(_product.WebDescription);
        Assert.NotNull(_product.WebRichDescription);
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
