using Product.Import.Json.Test.Common;


namespace Ozon.Product.Model.Test;

public partial class OzonLoadProductByJsonSettingsTest
{       
    [Fact]
    public void LoadOzonProductFromFullJson()
    {
        var product = TestHelper.GetItemFromJson<TestProduct>("Content/ozon.product.full.json", "Content/ozon.product.settings.json");

        Assert.NotNull(product);
        Assert.Equal("Средство для снятия макияжа", product.ProductType, ignoreCase: true);
        Assert.Contains("суфле", product.Description);
        Assert.True(product.Name?.Contains("суфле", StringComparison.InvariantCultureIgnoreCase));
        Assert.True(product.Country?.Contains("Россия", StringComparison.InvariantCultureIgnoreCase));
        Assert.NotEmpty(product.Purposes);
    }

    [Fact]
    public void LoadOzonProductFromShortJson()
    {
        var product = TestHelper.GetItemFromJson<TestProduct>("Content/ozon.product.short.json", "Content/ozon.product.settings.json");

        Assert.NotNull(product);
        Assert.Equal("Средство для снятия макияжа", product.ProductType, ignoreCase: true);
        Assert.Contains("суфле", product.Description);
        Assert.True(product.Name?.Contains("суфле", StringComparison.InvariantCultureIgnoreCase));
    }
}