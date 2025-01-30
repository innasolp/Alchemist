using System.Text.Json;

namespace Alchemist.Product.Shop.Ozon.Model.Tests;

public class OzonCategoryTest
{
    private readonly Category? _category;

    public OzonCategoryTest()
    {
        using var stream = File.OpenRead("OzonCategory.json");
        _category = JsonSerializer.Deserialize<Category>(stream);
        stream.Close();
    }

    [Fact]
    public void DeserializeCategoryProductsFromJson()
    {
        Assert.NotNull(_category);
        Assert.NotNull(_category.CategoryContent);
        Assert.NotNull(_category.CategoryContent.Items);
    }

    [Fact]
    public void CheckProductItemsPrices()
    {
        Assert.NotNull(_category?.CategoryContent?.Items);
        Assert.True(_category.CategoryContent.Items.All(i=>i.Price > 0));      
    }
}