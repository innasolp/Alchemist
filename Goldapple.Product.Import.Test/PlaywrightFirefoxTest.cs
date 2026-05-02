using BrowserDataLoader.Firefox.Standart.Windows;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Firefox.Windows.Standart;
using BrowserLauncher.Interfaces;
using Product.Import.Test;
using System.Reflection;
using WebLoader.Playwright.Firefox;
using Xunit.Abstractions;

namespace Goldapple.Product.Import.Test;

public class PlaywrightFirefoxTest : ProductShopTest
{
    private readonly string _categoryUrl = "https://goldapple.ru/front/api/catalog/cards-list?locale=ru";
    private readonly string _productUrl = "https://goldapple.ru/front/api/catalog/product-card/base?itemId=99730300001&cityId=0c5b2444-70a0-4932-980c-b4dc0d3f02b5&customerGroupId=0";

    private readonly ITestOutputHelper _testOutputHelper;
    
    private readonly string _requestHeadersFileName = "GoldApple.Headers.Firefox.json";

    private readonly string _requestHeadersPath;

    private readonly IBrowserDataLoader _browserDataLoader = new FirefoxStandartDataLoader();

    protected override IBrowserDataLoader BrowserDataLoader => _browserDataLoader;

    private readonly IBrowserLauncher _browserLauncher = new FirefoxStandartBrowserLauncher();

    protected override IBrowserLauncher BrowserLauncher => _browserLauncher;

    public PlaywrightFirefoxTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _requestHeadersPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_requestHeadersFileName}";       
    }

    [Fact]
    public async Task LoadGoldAppleCategoryPageTestAsync()
    {
        var data = await BrowserDataLoader.LoadCookies("goldapple.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidDataException(data.GetType().Name);

        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersPath, cookies);
        var content = new
        {
            categoryId = 1000000276,
            mode = "simple",
            pageNumber = 11,
            pageSize = 24,
            cityId = "555e7d61-d9a7-4ba6-9770-6caa8198c483"
        };

        await using var webLoader = new PlaywrightFirefoxLoader();

        await webLoader.Start();

        var stream = await webLoader.LoadFromApiRequestAsync(_categoryUrl, HttpMethod.Post, requestHeaders, content);
        stream.Close();

        await webLoader.Close();
    }

    [Fact]
    public async Task LoadGoldAppleProductPageTestAsync()
    {
        var data = await BrowserDataLoader.LoadCookies("goldapple.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidDataException(data.GetType().Name);

        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersPath, cookies);

        await using var webLoader = new PlaywrightFirefoxLoader();

        await webLoader.Start();

        var stream = await webLoader.LoadFromApiRequestAsync(_productUrl, headers: requestHeaders);
        stream.Close();

        await webLoader.Close();
    }
}