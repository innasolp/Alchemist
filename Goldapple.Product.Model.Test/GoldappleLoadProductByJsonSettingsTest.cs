using Product.Import.Json.Test.Common;

namespace Goldapple.Product.Model.Test;

public partial class GoldappleLoadProductByJsonSettingsTest
{       
    [Fact]
    public void LoadGoldappleProductFromJson()
    {
        var product = TestHelper.GetItemFromJson<Alchemist.Import.Products.Json.Product>("Content/goldapple.product.json", "Content/goldapple.product.settings.json");

        Assert.NotNull(product);
        Assert.Equal("ќчищающа€ крем-маска дл€ лица", product.ProductType, ignoreCase: true);
        Assert.Contains("ћ€гка€ и шелковиста€ текстура", product.Description);
        Assert.Contains("TEN SCIENCE", product.Brand, StringComparison.InvariantCultureIgnoreCase);
        Assert.True(product.Name?.Contains("AGE LUMINA", StringComparison.InvariantCultureIgnoreCase));
        Assert.True(product.Country?.Contains("»тали€", StringComparison.InvariantCultureIgnoreCase));
        Assert.NotEmpty(product.Purposes);
        Assert.Contains("очищение", product.Purposes);
    }
}