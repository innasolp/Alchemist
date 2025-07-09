using Alchemist.Import.Settings.Test.Model;
using Json.FileExtensions;

namespace Alchemist.Import.CategoriesSettings.Tests;

public class CategoriesSerializationTests
{
    private readonly string _categoriesJsonFileName = "shopCategories.json";

    [Fact]
    public async Task TestJsonDeserialization()
    {
        var settings = await _categoriesJsonFileName.ReadFromJsonFileAsync<TestCategoryShopImportSettings[]>();

        Assert.NotNull(settings);
        Assert.Equal(2, settings.Length);
        Assert.NotEmpty(settings[0].Services);
        Assert.NotEmpty(settings[1].Services);
    }
}