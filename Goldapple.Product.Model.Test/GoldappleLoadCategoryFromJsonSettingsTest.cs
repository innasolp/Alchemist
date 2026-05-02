using Alchemist.Import.Products.Json;
using Product.Import.Json.Test.Common;

namespace Goldapple.Product.Model.Test;

public class GoldappleLoadCategoryFromJsonSettingsTest
{
    [Fact]
    public void LoadGoldappleCategoryFromJson()
    {
        var category = TestHelper.GetItemFromJson<CategoryProducts>("Content/goldapple.category.json", "Content/goldapple.category.settings.json");

        Assert.NotNull(category);
        Assert.True(category.TotalCount > 0);
        Assert.Equal(24, category.CategoryProductItems.Length); 
        Assert.True(category.CategoryProductItems.All(c=>c != null));
        Assert.True(category.CategoryProductItems.All(c=>!string.IsNullOrEmpty(c.Name)));
        Assert.True(category.CategoryProductItems.All(c=>!string.IsNullOrEmpty(c.Id)));
        Assert.True(category.CategoryProductItems.All(c=>!string.IsNullOrEmpty(c.ItemPath)));

        Assert.True(category.CategoryProductItems.All(c => c.Price > 0));
        Assert.True(category.CategoryProductItems.All(c => !string.IsNullOrEmpty(c.Currency)));
    }
}