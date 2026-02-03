using Alchemist.Import.Category.Interfaces;
using Import.Html;
using Import.Html.Factory;
using Product.Import.Test;
using ShopImport.Category.Recursive;
using System.Collections.ObjectModel;
using System.Text.Json;
using WebLoader.Interfaces;
using WebLoader.Playwright.ChromiumPatchright;
using Xunit.Abstractions;

namespace Ozon.Product.Import.Category.Test;

public class ChromiumPatchrightTest(ITestOutputHelper testOutputHelper)
{
    private readonly string _shopCategoryApiUrlFormat = "https://www.ozon.ru/api/composer-api.bx/_action/v2/categoryChildV3?menuId=185&categoryId={0}";
    private readonly string[] _nodePath = ["data", "columns", "categories"];

    private const string TimeWatchMessageFormat = "{0} load time {1} seconds";

    private readonly Dictionary<string, PropertyPath> _categoryPropertyPathes = new()
    {
        { "Url", new PropertyPath("Url","url") },
        {"Description",new PropertyPath("Description","title") },
        {"Name",new PropertyPath("Name","title") },
        {"Children",new PropertyPath("Children","categories") }
    };

    private readonly string _shopUrl = "https://www.ozon.ru/?__rr=1";

    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;    

    private async Task<JsonDocument?> GetJsonDocumentAsync(IWebLoader webLoader)
    {
        var htmlSearcher = HtmlSearchFactory.CreateSearcher(SearchMatchType.Like);

        var token = new CancellationTokenSource();

        using var stream = await webLoader.LoadFromApiUrl(_shopUrl);
        var values = await htmlSearcher.GetValues(stream, new HtmlSearchOptions
        {
            Tag = "div",
            SearchString = "id=state-catalogMenu",
            ValueString = "data-state",
        }, token.Token);
        stream.Close();

        Assert.True(values.Count > 0);

        var document = JsonDocument.Parse(values[0]);

        return await Task.FromResult(document);
    }


    [Fact]
    public async Task LoadCategoriesToCollectionTestAsync()
    {
        var routeUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2FsearchSuggestions";

        await using var webLoader = new ChromiumPatchrightWebLoader();
        await webLoader.Start();

        var (document, timespan) = await TimeWatchHelper.ExecuteTaskWithTimeWatchAsync(async () =>
        {
            await webLoader.WaitForUrl(_shopUrl, url=>url.Contains(routeUrl), timeoutInMilliseconds : 5000);

            return await GetJsonDocumentAsync(webLoader);
        });

        testOutputHelper.WriteLine(TimeWatchMessageFormat, "json", timespan.TotalSeconds);


        Assert.NotNull(document);

        var categories = new ObservableCollection<ICategory>();
        var tokenSource = new CancellationTokenSource();
        var jsonElementHelper = new JsonElementHelper();

        timespan = await TimeWatchHelper.ExecuteTaskWithTimeWatchAsync(async () =>
        {
            await RecursiveCategory.LoadAllChildrenAsync(null, categories, document.RootElement, _nodePath,
                _categoryPropertyPathes, jsonElementHelper, tokenSource.Token);
        });

        testOutputHelper.WriteLine(TimeWatchMessageFormat, "Parent categories", timespan.TotalSeconds);

        Assert.True(categories.Count > 0);


        var parentCategories = new List<ICategory>(categories);

        timespan = await TimeWatchHelper.ExecuteTaskWithTimeWatchAsync(async () =>
        {
            foreach (var parentCategory in parentCategories)
            {
                var url = string.Format(_shopCategoryApiUrlFormat, parentCategory.Id);
                using var categoryStream = await webLoader.LoadFromApiRequestAsync(url);

                var categoriesJson = await JsonDocument.ParseAsync(categoryStream);

                categoryStream.Close();

                await RecursiveCategory.LoadAllChildrenAsync(parentCategory as RecursiveCategory, categories, categoriesJson.RootElement,
                    _nodePath, _categoryPropertyPathes, jsonElementHelper, tokenSource.Token);
            }
        });

        await webLoader.Close();

        testOutputHelper.WriteLine(TimeWatchMessageFormat, "children categories", timespan.TotalSeconds);

        Assert.Equal(parentCategories.Count, categories.Count(c => c.ParentId == null));

        _testOutputHelper.WriteLine($"parent categories {parentCategories.Count}");
        _testOutputHelper.WriteLine($"all categories {categories.Count}");
    }
}
