using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Alchemist.Import.Category.Service.Json;
using BrowserDataLoader.Interfaces;
using System.Collections.ObjectModel;
using System.Text.Json;
using WebLoader.Interfaces;
using Xunit.Abstractions;

namespace Alchemist.Import.Html.Categories.Tests;

public class GoldAppleCategoriesLoadTest
{
    private readonly IBrowserDataLoader _dataLoader;

    private readonly IWebLoader _webLoader;
    
    private readonly string _shopCategoriesUrl = "https://goldapple.ru/web/api/v3/catalog/navigation";    

    private readonly string requestHeadersFileName = "GoldApple.Headers.Firefox.json";
    string[] _nodePath = ["general"];

    private readonly ITestOutputHelper _testOutputHelper;

    private readonly Dictionary<string, PropertyPath> _categoryPropertyPathes = new Dictionary<string, PropertyPath>()
        {
            { "Url",new PropertyPath("Url","link",true) },
            {"Description",new PropertyPath("Description","name") },
            {"Id",new PropertyPath("Id","id") },
            {"Children",new PropertyPath("Children","children") }
        };

    public GoldAppleCategoriesLoadTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _dataLoader = new BrowserDataLoader.Firefox.Standart.Windows.FirefoxStandartDataLoader();
        _webLoader = new WebLoader.Playwright.Firefox.PlaywrightFirefoxLoader();
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

        
        var cookies = await _dataLoader.LoadCookies();
        var requestHeaders = HeadersHelper.LoadHeadersForRequest(requestHeadersFileName, cookies);

        using var stream = await _webLoader.LoadFromUrl(_shopCategoriesUrl, requestHeaders);
        var jsonDocument = await JsonSerializer.DeserializeAsync<JsonDocument>(stream);
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
        Assert.Equal(14, categories.Count(c => c.ParentId == null));

        _testOutputHelper.WriteLine($"parent categories {categories.Count(c => c.ParentId == null)}");
        _testOutputHelper.WriteLine($"all categories {categories.Count}");

        await Task.Delay(1000);
    }    
}