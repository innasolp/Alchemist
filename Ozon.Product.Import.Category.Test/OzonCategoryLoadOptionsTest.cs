using Alchemist.Import.Category.Service;
using Import.Html;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ozon.Product.Import.Category.Test;

public class OzonCategoryLoadOptionsTest
{
    private readonly string _nonHeaderOptionsfileName = "Ozon.CategoryLoadNonHeaderOptions.Firefox.json";

    private readonly string _shopCategoryApiUrlFormat = "https://www.ozon.ru/api/composer-api.bx/_action/v2/categoryChildV3?menuId=185&categoryId={0}";

    private readonly Dictionary<string, PropertyPath> _categoryPropertyPathes = new()
        {
            { "Url", new PropertyPath("Url","url") },
            {"Description",new PropertyPath("Description","title") },
            {"Children",new PropertyPath("Children","categories") }
        };

    private readonly HtmlSearchOptions _htmlSearchOptions = new HtmlSearchOptions
    {
        Tag = "div",
        SearchString = "id=state-catalogMenu",
        ValueString = "data-state",
    };

    private void AssertOptions(CategoryLoadOptions categoryLoadOptions)
    {
        Assert.NotNull(categoryLoadOptions);

        Assert.Equal(categoryLoadOptions.HtmlSearchOptions.ValueString, _htmlSearchOptions.ValueString);
        Assert.Equal(categoryLoadOptions.HtmlSearchOptions.Tag, _htmlSearchOptions.Tag);
        Assert.Equal(categoryLoadOptions.HtmlSearchOptions.SearchString, _htmlSearchOptions.SearchString);

        Assert.Equal(categoryLoadOptions.CategoriesApiUrlFormat, _shopCategoryApiUrlFormat);

        Assert.NotNull(categoryLoadOptions.CategoryPropertyPaths);
        Assert.True(categoryLoadOptions.CategoryPropertyPaths.ContainsKey("Url"));
        Assert.Equal(categoryLoadOptions.CategoryPropertyPaths["Url"].Path, _categoryPropertyPathes["Url"].Path);
    }

    private async Task<CategoryLoadOptions?> LoadOptionsAsync(string fileName)
    {
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true
        };

        using var s = File.OpenRead($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{fileName}");

        var categoryLoadOptions = await JsonSerializer.DeserializeAsync<CategoryLoadOptions>(s);

        s.Close();

        return await Task.FromResult(categoryLoadOptions);
    }

    [Fact]
    public async Task LoadOptionsTestAsync()
    {
        var categoryLoadOptions = await LoadOptionsAsync(_nonHeaderOptionsfileName);

        Assert.NotNull(categoryLoadOptions);

        AssertOptions(categoryLoadOptions);
    }
    
}
