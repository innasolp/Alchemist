using Alchemist.Import.Category.Json;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using WebLoader.Interfaces;

namespace Alchemist.Import.Html.Categories.Tests;

public class OzonCategoriesLoadTest
{    
    private readonly string _requestHeadersStandartFileName = "Ozon.Headers.Firefox.Standart.json";
    
    private readonly HtmlSearchFactory _htmlSearchFactory = new();

    private readonly string _shopCategoryApiUrlFormat = "https://www.ozon.ru/api/composer-api.bx/_action/v2/categoryChildV3?menuId=185&categoryId={0}";
    private readonly string[] _nodePath = ["data", "columns", "categories"];

    private readonly Dictionary<string, PropertyPath> _categoryPropertyPathes = new()
    {
        { "Url", new PropertyPath("Url","url") },
        {"Description",new PropertyPath("Description","title") },
        {"Children",new PropertyPath("Children","categories") }
    };

    private readonly string _shopUrl = "https://www.ozon.ru/?__rr=1";

    private static async Task<IWebLoader> CreateWebLoaderAsync(string requestHeadersFileName)
    {
        using var s = File.OpenRead($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{requestHeadersFileName}");

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(s);

        s.Close();

        if (requestHeaders == null)
            Assert.Fail("request header not loaded");

        var browserDataLoader = new BrowserDataLoader.Firefox.Standart.Windows.FirefoxStandartDataLoader();
        var cookies = await browserDataLoader.LoadCookies();
        Assert.True(cookies.Count > 0);
        Assert.True(cookies.All(c => c.Value != null));

        var webLoader = new WebLoader.Playwright.Firefox.PlaywrightFirefoxLoader(browserDataLoader);
        var result = await webLoader.Start(requestHeaders);
        Assert.True(result);

        return webLoader;
    }

    private async Task<JsonDocument?> GetJsonDocumentAsync(IWebLoader webLoader)
    {
        var htmlSearcher = _htmlSearchFactory.CreateSearcher(SearchMatchType.Like);

        using var stream = await webLoader.LoadFromUrl(_shopUrl);
        var values = await htmlSearcher.GetValues(stream, new HtmlSearchOptions
        {
            Tag = "div",
            SearchString = "id=state-catalogMenu",
            ValueString = "data-state",
        });
        stream.Close();

        Assert.True(values.Count > 0);

        var document = JsonDocument.Parse(values[0]);

        return await Task.FromResult(document);
    }

    [Fact]
    public async Task StandartLoadCategoriesListTestAsync()
    {        
        var webLoader = await CreateWebLoaderAsync(_requestHeadersStandartFileName);
        
        var document = await GetJsonDocumentAsync(webLoader);

        Assert.NotNull(document);
        
        var parentCategories = JsonCategory.LoadAllChildren(null, document.RootElement,
            _nodePath,
            _categoryPropertyPathes);

        Assert.True(parentCategories.Count > 0);

        var endCategories = new List<JsonCategory>();
        foreach(var parentCategory in parentCategories)
        {
            var url = string.Format(_shopCategoryApiUrlFormat, parentCategory.Id);
            using var categoryStream = await webLoader.LoadFromUrl(url);
            
            var categoriesJson = await JsonDocument.ParseAsync(categoryStream);

            categoryStream.Close();

            endCategories.AddRange(JsonCategory.LoadAllChildren(parentCategory, categoriesJson.RootElement, _nodePath, _categoryPropertyPathes));
        }

        Assert.True(endCategories.Count > 0);
    }

    [Fact]
    public async Task LoadCategoriesToCollectionTestAsync()
    {
        var webLoader = await CreateWebLoaderAsync(_requestHeadersStandartFileName);

        var document = await GetJsonDocumentAsync(webLoader);

        Assert.NotNull(document);

        var categories = new ObservableCollection<JsonCategory>();

        JsonCategory.LoadAllChildren(null, categories, document.RootElement,
            _nodePath,
            _categoryPropertyPathes);

        Assert.True(categories.Count > 0);
        var parentCount = categories.Count;

        var parentCategories = new List<JsonCategory>(categories);
        foreach (var parentCategory in parentCategories)
        {
            var url = string.Format(_shopCategoryApiUrlFormat, parentCategory.Id);
            using var categoryStream = await webLoader.LoadFromUrl(url);

            var categoriesJson = await JsonDocument.ParseAsync(categoryStream);

            categoryStream.Close();

            JsonCategory.LoadAllChildren(parentCategory, categories, categoriesJson.RootElement, _nodePath, _categoryPropertyPathes);
        }

        Assert.True(categories.Count > parentCount);       
    }
}