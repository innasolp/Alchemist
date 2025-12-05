using Import.Html.Factory;
using BrowserDataLoader.Interfaces;
using System.Collections.ObjectModel;
using System.Text.Json;
using WebLoader.Interfaces;
using Xunit.Abstractions;
using Import.Html;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Alchemist.Import.Category.Service.Json;

namespace Alchemist.Import.Html.Categories.Tests;

public class OzonCategoriesLoadTest(ITestOutputHelper testOutputHelper)
{    
    private readonly string _requestHeadersStandartFileName = "Ozon.Headers.Firefox.Standart.json";   

    private readonly string _shopCategoryApiUrlFormat = "https://www.ozon.ru/api/composer-api.bx/_action/v2/categoryChildV3?menuId=185&categoryId={0}";
    private readonly string[] _nodePath = ["data", "columns", "categories"];

    private const string TimeWatchMessageFormat = "{0} load time {1} seconds";
    
    private readonly Dictionary<string, PropertyPath> _categoryPropertyPathes = new()
    {
        { "Url", new PropertyPath("Url","url") },
        {"Description",new PropertyPath("Description","title") },
        {"Children",new PropertyPath("Children","categories") }
    };

    private readonly string _shopUrl = "https://www.ozon.ru/?__rr=1";

    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    private readonly IBrowserDataLoader _browserDataLoader = new BrowserDataLoader.Firefox.Standart.Windows.FirefoxStandartDataLoader();

    private static async Task<IWebLoader> CreateWebLoaderAsync()
    {
        var webLoader = new WebLoader.Playwright.Firefox.PlaywrightFirefoxLoader();
        var result = await webLoader.Start();
        Assert.True(result);

        return webLoader;
    }

    private async Task<JsonDocument?> GetJsonDocumentAsync(IWebLoader webLoader, 
        Dictionary<string,string> requestHeaders)
    {
        var htmlSearcher = HtmlSearchFactory.CreateSearcher(SearchMatchType.Like);

        var token = new CancellationTokenSource();

        using var stream = await webLoader.LoadFromUrl(_shopUrl, requestHeaders);
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
        var webLoader = await CreateWebLoaderAsync();

        var ((document, requestHeaders), timespan) = await TimeWatchHelper.ExecuteTaskWithTimeWatchAsync(async () =>
        {
            var cookies = await _browserDataLoader.LoadCookies();
            var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersStandartFileName, cookies);
            return (await GetJsonDocumentAsync(webLoader, requestHeaders), requestHeaders);
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
                using var categoryStream = await webLoader.LoadFromUrl(url, requestHeaders);

                var categoriesJson = await JsonDocument.ParseAsync(categoryStream);

                categoryStream.Close();

                await RecursiveCategory.LoadAllChildrenAsync(parentCategory as RecursiveCategory, categories, categoriesJson.RootElement,
                    _nodePath, _categoryPropertyPathes, jsonElementHelper, tokenSource.Token);
            }
        });

        testOutputHelper.WriteLine(TimeWatchMessageFormat, "children categories", timespan.TotalSeconds);

        Assert.Equal(parentCategories.Count, categories.Count(c=>c.ParentId == null));

        _testOutputHelper.WriteLine($"parent categories {parentCategories.Count}");
        _testOutputHelper.WriteLine($"all categories {categories.Count}");
    }
}