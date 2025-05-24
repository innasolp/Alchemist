using Alchemist.Import.Category.Json;
using BrowserDataLoader.Interfaces;
using Json.FileExtensions;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using WebLoader.Common;
using WebLoader.Interfaces;
using Xunit.Abstractions;

namespace Alchemist.Import.Html.Categories.Tests;

public class GoldAppleCategoriesLoadTest
{
    private readonly IBrowserDataLoader _dataLoader;

    private readonly IWebLoader _webLoader;
    
    private readonly string _shopCategoriesUrl = "https://goldapple.ru/front/api/catalog/navigation";    

    private readonly string requestHeadersFileName = "GoldApple.Headers.Firefox.json";

    private readonly string _requestHeadersPath;
    string[] _nodePath = ["data"];

    private readonly ITestOutputHelper _testOutputHelper;

    private readonly Dictionary<string, PropertyPath> _categoryPropertyPathes = new Dictionary<string, PropertyPath>()
        {
            { "Url",new PropertyPath("Url","link",true) },
            {"Description",new PropertyPath("Description","name") },
            {"Id",new PropertyPath("Id","id") },
            {"Children",new PropertyPath("Children","children") },
            {"IsParented",new PropertyPath("IsParented","parent") }
        };

    public GoldAppleCategoriesLoadTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _dataLoader = new BrowserDataLoader.Firefox.Standart.Windows.FirefoxStandartDataLoader();
        _webLoader = new WebLoader.Playwright.Firefox.PlaywrightFirefoxLoader(_dataLoader);

        _requestHeadersPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{requestHeadersFileName}";
    }

    private async Task InitializeAsync()
    {
        var result = await _webLoader.Start();
        Assert.True(result);
    }

    [Fact]
    public async Task LoadCategoriesAsync()
    {
        if (!_webLoader.IsStarted)
            await InitializeAsync();

        var requestHeaders = await _requestHeadersPath.ReadFromJsonFileAsync<RequestHeaders>();

        Assert.NotNull(requestHeaders);
        Assert.NotNull(requestHeaders.Headers);

        var cookies = await _dataLoader.LoadCookies();
        Assert.True(cookies.Count > 0);
        Assert.True(cookies.All(c => c.Value != null));

        using var stream = await _webLoader.LoadFromUrl(_shopCategoriesUrl, requestHeaders);
        var jsonDocument = await JsonSerializer.DeserializeAsync<JsonDocument>(stream);
        stream.Close();       

        Assert.NotNull(jsonDocument);

        var categories = new ObservableCollection<JsonCategory>();
        var tokenSource = new CancellationTokenSource();

        await JsonCategoryAsync.LoadAllChildrenAsync(null, categories, jsonDocument.RootElement,
               _nodePath,
               _categoryPropertyPathes,
               tokenSource.Token);

        Assert.True(categories.Count > 0);

        Assert.NotEmpty(categories.Where(c => c.ParentId > 0));
        Assert.Equal(14, categories.Count(c => c.ParentId == null));

        _testOutputHelper.WriteLine($"parent categories {categories.Count(c => c.ParentId == null)}");
        _testOutputHelper.WriteLine($"all categories {categories.Count}");
    }
    [Fact]
    public async Task LoadCategoriesSync()
    {
        if (!_webLoader.IsStarted)
            await InitializeAsync();

        var requestHeaders = await _requestHeadersPath.ReadFromJsonFileAsync<RequestHeaders>();

        Assert.NotNull(requestHeaders);
        Assert.NotNull(requestHeaders.Headers);

        var cookies = await _dataLoader.LoadCookies();
        Assert.True(cookies.Count > 0);
        Assert.True(cookies.All(c => c.Value != null));

        using var stream = await _webLoader.LoadFromUrl(_shopCategoriesUrl, requestHeaders);
        var jsonDocument = await JsonSerializer.DeserializeAsync<JsonDocument>(stream);
        stream.Close();

        Assert.NotNull(jsonDocument);

        var categories = new ObservableCollection<JsonCategory>();        

        JsonCategory.LoadAllChildren(null, categories, jsonDocument.RootElement,
               _nodePath,
               _categoryPropertyPathes);

        Assert.True(categories.Count > 0);

        Assert.NotEmpty(categories.Where(c => c.ParentId > 0));
        Assert.Equal(14, categories.Count(c => c.ParentId == null));

        _testOutputHelper.WriteLine($"parent categories {categories.Count(c => c.ParentId == null)}");
        _testOutputHelper.WriteLine($"all categories {categories.Count}");
    }

}