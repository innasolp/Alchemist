using Alchemist.Import.Category.Interfaces;
using ShopImport.Category.Recursive;
using System.Collections.ObjectModel;
using System.Text.Json;
using WebLoader.Playwright.ChromiumPatchright;
using Xunit.Abstractions;

namespace Goldapple.Product.Import.Category.Test;

public class ChromiumPatchrightTest(ITestOutputHelper testOutputHelper)
{
    private readonly string _shopCategoriesApiUrl = "https://goldapple.ru/web/api/v3/catalog/navigation";
    private readonly string _shopUrl = "https://goldapple.ru";

    private readonly string[] _nodePath = ["general"];

    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    private readonly Dictionary<string, PropertyPath> _categoryPropertyPathes = new Dictionary<string, PropertyPath>()
        {
            { "Url",new PropertyPath("Url","link",true) },
            {"Description",new PropertyPath("Description","name") },
            {"Name",new PropertyPath("Name","name") },
            {"Id",new PropertyPath("Id","id") },
            {"Children",new PropertyPath("Children","children") }
        };

    [Fact]
    public async Task LoadCategoriesAsync()
    {
        JsonDocument? jsonDocument;

        await using var webLoader = new ChromiumPatchrightWebLoader();
        var result = await webLoader.Start();

        var (success, stream) = await webLoader.TryLoadFromRoute(_shopUrl, (url) => url.Contains(_shopCategoriesApiUrl),
            timeoutInMilliseconds: 15000);
        
        Assert.True(success);

        using (stream)

        jsonDocument = await JsonSerializer.DeserializeAsync<JsonDocument>(stream);
        stream.Close();

        Assert.NotNull(jsonDocument);

        var categories = new ObservableCollection<ICategory>();
        var tokenSource = new CancellationTokenSource();
        var elementHelper = new JsonElementHelper();

        await RecursiveCategory.LoadAllChildrenAsync(null, categories, jsonDocument.RootElement,
               _nodePath,
               _categoryPropertyPathes,
               elementHelper,
               tokenSource.Token);        

        Assert.True(categories.Count > 0);

        Assert.NotEmpty(categories.Where(c => c.ParentId > 0));
        Assert.Equal(12, categories.Count(c => c.ParentId == null));

        _testOutputHelper.WriteLine($"parent categories {categories.Count(c => c.ParentId == null)}");
        _testOutputHelper.WriteLine($"all categories {categories.Count}");

        await webLoader.Close();
    }
}
