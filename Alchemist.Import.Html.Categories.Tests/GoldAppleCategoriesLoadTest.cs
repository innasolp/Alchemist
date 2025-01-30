using Alchemist.Import.Category.Json;
using BrowserDataLoader.Interfaces;
using Json.Extensions;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using WebLoader.Interfaces;

namespace Alchemist.Import.Html.Categories.Tests;

public class GoldAppleCategoriesLoadTest
{
    private readonly IBrowserDataLoader _dataLoader;

    private readonly IWebLoader _webLoader;
    
    private readonly string _shopUrl = "https://goldapple.ru";

    private readonly HtmlSearchFactory _htmlSearchFactory = new();

    private readonly string requestHeadersFileName = "GoldApple.Headers.Firefox.json";

    private readonly string _requestHeadersPath;

    string[] _nodePath = ["data"];

    private readonly Dictionary<string, PropertyPath> _categoryPropertyPathes = new Dictionary<string, PropertyPath>()
        {
            { "Url",new PropertyPath("Url","link",true) },
            {"Description",new PropertyPath("Description","name") },
            {"Id",new PropertyPath("Id","id") },
            {"Children",new PropertyPath("Children","children") },
            {"IsParented",new PropertyPath("IsParented","parent") }
        };

    public GoldAppleCategoriesLoadTest()
    {
        _dataLoader = new BrowserDataLoader.Firefox.Standart.Windows.FirefoxStandartDataLoader();
        _webLoader = new WebLoader.Playwright.Firefox.PlaywrightFirefoxLoader(_dataLoader);

        _requestHeadersPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{requestHeadersFileName}";
    }

    private async Task InitializeAsync()
    {
        var requestHeaders = await _requestHeadersPath.ReadFromJsonFileAsync<RequestHeaders>();

        Assert.NotNull(requestHeaders);
        Assert.NotNull(requestHeaders.Headers);
        
        var cookies = await _dataLoader.LoadCookies();
        Assert.True(cookies.Count > 0);
        Assert.True(cookies.All(c => c.Value != null));

        var result = await _webLoader.Start(requestHeaders);
        Assert.True(result);
    }

    [Fact]
    public async Task TestLoadCategoriesAsync()
    {
        if (!_webLoader.IsStarted)
            await InitializeAsync();

        var htmlSearcher = _htmlSearchFactory.CreateSearcher(SearchMatchType.Equals, SearchElementType.Value);

        using var stream = await _webLoader.LoadFromUrl(_shopUrl);
        var values = await htmlSearcher.GetValues(stream, new HtmlSearchOptions
        {
            Tag = "script",
            ValueString = "window.serverCache['navigation']",
        });
        stream.Close();

        Assert.True(values.Count > 0);

        var jsonDocument = JsonDocument.Parse(values[0]);

        Assert.NotNull(jsonDocument);

        var categories = new ObservableCollection<JsonCategory>();

        JsonCategory.LoadAllChildren(null, categories, jsonDocument.RootElement,
               _nodePath,
               _categoryPropertyPathes);

        Assert.True(categories.Count > 0);

        Assert.NotEmpty(categories.Where(c => c.ParentId != 0));
    }
}