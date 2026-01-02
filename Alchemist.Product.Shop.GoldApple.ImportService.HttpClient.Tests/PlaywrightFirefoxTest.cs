using Alchemist.Product.Shop.GoldApple.Model;
using Alchemist.Test.Product.Shop;
using BrowserDataLoader.Firefox.Standart.Windows;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Firefox.Windows.Standart;
using BrowserLauncher.Interfaces;
using System.Reflection;
using System.Text.Json;
using WebLoader.Playwright.Firefox;
using Xunit.Abstractions;

namespace Alchemist.Shop.GoldenApple.ImportService.HttpClient.Tests;

public class PlaywrightFirefoxTest : ProductShopTest
{
    private readonly string _categoryUrl = "https://goldapple.ru/front/api/catalog/cards-list?locale=ru";// "https://goldapple.ru/front/api/catalog/products?categoryId=1000000252&cityId=555e7d61-d9a7-4ba6-9770-6caa8198c483&cityDistrict=%D0%9C%D0%BE%D1%81%D0%BA%D0%BE%D0%B2%D1%81%D0%BA%D0%B8%D0%B9&geoPolygons[]=EKB-000000288&geoPolygons[]=EKB-000000749&pageNumber=3";
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
        var data = await _browserServiceMock.Object.GetData("goldapple.ru");
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

        var webLoader = new PlaywrightFirefoxLoader();

        await webLoader.Start();

        var stream = await webLoader.LoadFromApiRequestAsync(_categoryUrl,HttpMethod.Post, requestHeaders, content);
        var category = await JsonSerializer.DeserializeAsync<CategoryProducts>(stream);
        stream.Close();

        Assert.NotNull(category);
        Assert.NotNull(category.Data);
        Assert.True(category.Data.Count > 0);
        Assert.NotEmpty(category.Data.Products);

        await webLoader.Close();
    }

    [Fact]
    public async Task LoadGoldAppleProductPageTestAsync()
    {
        var data = await _browserServiceMock.Object.GetData("goldapple.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidDataException(data.GetType().Name);

        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersPath, cookies);

        var webLoader = new PlaywrightFirefoxLoader();

        await webLoader.Start();

        var stream = await webLoader.LoadFromApiRequestAsync(_productUrl, headers: requestHeaders);
        var productData = await JsonSerializer.DeserializeAsync<ProductData>(stream);
        stream.Close();

        Assert.NotNull(productData);
        Assert.NotNull(productData.Data.Name);
        Assert.NotNull(productData.Data.ItemId);
        Assert.NotNull(productData.Data.ProductType);

        await webLoader.Close();
    }
}