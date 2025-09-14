using Alchemist.Import.Category.Json;
using Alchemist.Import.Html.Factory;
using BrowserDataLoader.Interfaces;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using WebLoader.Common;
using WebLoader.Interfaces;
using Xunit.Abstractions;

namespace Alchemist.Import.Html.Categories.Tests;

public class OzonCategoriesLoadTest(ITestOutputHelper testOutputHelper)
{    
    private readonly string _requestHeadersStandartFileName = "Ozon.Headers.Firefox.Standart.json";   

    private readonly string _shopCategoryApiUrlFormat = "https://www.ozon.ru/api/composer-api.bx/_action/v2/categoryChildV3?menuId=185&categoryId={0}";
    private readonly string[] _nodePath = ["data", "columns", "categories"];

    private readonly Dictionary<string, PropertyPath> _categoryPropertyPathes = new()
    {
        { "Url", new PropertyPath("Url","url") },
        {"Description",new PropertyPath("Description","title") },
        {"Children",new PropertyPath("Children","categories") }
    };

    private readonly string _shopUrl = "https://www.ozon.ru/?__rr=1";

    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    private readonly IBrowserDataLoader _browserDataLoader = new BrowserDataLoader.Firefox.DevEdition.Windows.FirefoxDevEditionDataLoader();

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
    public async Task StandartLoadCategoriesListTest()
    {        
        var webLoader = await CreateWebLoaderAsync();        

        var cookies = await _browserDataLoader.LoadCookies();
        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersStandartFileName, cookies);
        var document = await GetJsonDocumentAsync(webLoader, requestHeaders);

        Assert.NotNull(document);
        
        var parentCategories = JsonCategory.LoadAllChildren(null, document.RootElement,
            _nodePath,
            _categoryPropertyPathes);

        Assert.True(parentCategories.Count > 0);

        var endCategories = new List<JsonCategory>();
        foreach(var parentCategory in parentCategories)
        {
            var url = string.Format(_shopCategoryApiUrlFormat, parentCategory.Id);
            using var categoryStream = await webLoader.LoadFromUrl(url, requestHeaders);
            
            var categoriesJson = await JsonDocument.ParseAsync(categoryStream);

            categoryStream.Close();

            endCategories.AddRange(JsonCategory.LoadAllChildren(parentCategory, categoriesJson.RootElement, _nodePath, _categoryPropertyPathes));
        }

        Assert.True(endCategories.Count > 0);

        _testOutputHelper.WriteLine($"parent categories {parentCategories.Count}");
        _testOutputHelper.WriteLine($"all categories {endCategories.Count}");
    }

    [Fact]
    public async Task LoadCategoriesToCollectionTestAsync()
    {
        var webLoader = await CreateWebLoaderAsync();

        var cookies = await _browserDataLoader.LoadCookies();
        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersStandartFileName, cookies);
        var document = await GetJsonDocumentAsync(webLoader, requestHeaders);

        Assert.NotNull(document);

        var categories = new ObservableCollection<JsonCategory>();

        var tokenSource = new CancellationTokenSource();

        await JsonCategoryAsync.LoadAllChildrenAsync(null, categories, document.RootElement,
            _nodePath,
            _categoryPropertyPathes,
            tokenSource.Token);

        Assert.True(categories.Count > 0);
        
        var parentCategories = new List<JsonCategory>(categories);
        foreach (var parentCategory in parentCategories)
        {
            var url = string.Format(_shopCategoryApiUrlFormat, parentCategory.Id);
            using var categoryStream = await webLoader.LoadFromUrl(url, requestHeaders);

            var categoriesJson = await JsonDocument.ParseAsync(categoryStream);

            categoryStream.Close();

            await JsonCategoryAsync.LoadAllChildrenAsync(parentCategory, categories, categoriesJson.RootElement, _nodePath, _categoryPropertyPathes, tokenSource.Token);
        }

        Assert.Equal(parentCategories.Count, categories.Count(c=>c.ParentId == null));

        _testOutputHelper.WriteLine($"parent categories {parentCategories.Count}");
        _testOutputHelper.WriteLine($"all categories {categories.Count}");
    }

    [Fact]
    public async Task LoadCategoriesToCollectionTestSync()
    {
        var webLoader = await CreateWebLoaderAsync();        

        var cookies = await _browserDataLoader.LoadCookies();
        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersStandartFileName, cookies);
        var document = await GetJsonDocumentAsync(webLoader, requestHeaders);

        Assert.NotNull(document);

        var categories = new ObservableCollection<JsonCategory>();        

        JsonCategory.LoadAllChildren(null, categories, document.RootElement,
            _nodePath,
            _categoryPropertyPathes);

        Assert.True(categories.Count > 0);

        var parentCategories = new List<JsonCategory>(categories);
        foreach (var parentCategory in parentCategories)
        {
            var url = string.Format(_shopCategoryApiUrlFormat, parentCategory.Id);
            using var categoryStream = await webLoader.LoadFromUrl(url, requestHeaders);

            var categoriesJson = await JsonDocument.ParseAsync(categoryStream);

            categoryStream.Close();

            JsonCategory.LoadAllChildren(parentCategory, categories, categoriesJson.RootElement, _nodePath, _categoryPropertyPathes);
        }

        Assert.Equal(parentCategories.Count, categories.Count(c => c.ParentId == null));

        _testOutputHelper.WriteLine($"parent categories {parentCategories.Count}");
        _testOutputHelper.WriteLine($"all categories {categories.Count}");
    }
}