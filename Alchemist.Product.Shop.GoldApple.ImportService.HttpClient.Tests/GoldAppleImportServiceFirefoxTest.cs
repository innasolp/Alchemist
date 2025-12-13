using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.Shop.GoldApple.Model;
using Alchemist.Test.Product.Shop;
using BrowserDataLoader.Firefox.Standart.Windows;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Firefox.Windows.Standart;
using BrowserLauncher.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebLoader.Common;
using WebLoader.Interfaces;
using WebLoader.Playwright.Firefox;
using Xunit.Abstractions;

namespace Alchemist.Shop.GoldenApple.ImportService.HttpClient.Tests;

public class GoldAppleImportServiceFirefoxTest : ProductShopTest
{
    private readonly string _categoryUrl = "https://goldapple.ru/front/api/catalog/cards-list?locale=ru";// "https://goldapple.ru/front/api/catalog/products?categoryId=1000000252&cityId=555e7d61-d9a7-4ba6-9770-6caa8198c483&cityDistrict=%D0%9C%D0%BE%D1%81%D0%BA%D0%BE%D0%B2%D1%81%D0%BA%D0%B8%D0%B9&geoPolygons[]=EKB-000000288&geoPolygons[]=EKB-000000749&pageNumber=3";
    private readonly string _productUrl = "https://goldapple.ru/front/api/catalog/product-card/base?itemId=99730300001&cityId=0c5b2444-70a0-4932-980c-b4dc0d3f02b5&customerGroupId=0";

    private readonly IWebLoader _webLoader;
    private readonly ITestOutputHelper _testOutputHelper;
    
    private readonly string _requestHeadersFileName = "GoldApple.Headers.Firefox.json";

    private readonly string _requestHeadersPath;

    private readonly IBrowserDataLoader _browserDataLoader = new FirefoxStandartDataLoader();

    protected override IBrowserDataLoader BrowserDataLoader => _browserDataLoader;

    private readonly IBrowserLauncher _browserLauncher = new FirefoxStandartBrowserLauncher();

    protected override IBrowserLauncher BrowserLauncher => _browserLauncher;

    private async Task<Dictionary<string, string>> GetRequestHeadersAsync(string requestHeadersPath)
    {
        var data = await _browserServiceMock.Object.GetData("goldapple.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidDataException(data.GetType().Name);

        return HeadersHelper.LoadHeadersForRequest(requestHeadersPath, cookies);
    }


    public GoldAppleImportServiceFirefoxTest(ITestOutputHelper testOutputHelper)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddHttpClient();
        var host = builder.Build();        

        _testOutputHelper = testOutputHelper;
        _webLoader = new PlaywrightFirefoxLoader();  
        _requestHeadersPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_requestHeadersFileName}";       
    }


    private async Task InitWebLoaderIfNeedAsync()
    {
        if (_webLoader.IsStarted) return;

        var result = await _webLoader.Start();
        Assert.True(result);
    }

    [Fact]
    public async Task LoadGoldAppleCategoryPageTestAsync()
    {
        await InitWebLoaderIfNeedAsync();
        
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


        var stream = await _webLoader.LoadFromUrl(_categoryUrl, requestHeaders, HttpMethod.Post, content);
        var category = await JsonSerializer.DeserializeAsync<CategoryProducts>(stream);
        stream.Close();

        Assert.NotNull(category);
        Assert.NotNull(category.Data);
        Assert.True(category.Data.Count > 0);
        Assert.NotEmpty(category.Data.Products);
       
        var productUrlFormat = "https://goldapple.ru/front/api/catalog/product-card/base/v3?itemId={0}&cityId=0c5b2444-70a0-4932-980c-b4dc0d3f02b5&customerGroupId=0";

        using var appInfoStream = await _webLoader.LoadFromUrl("https://goldapple.ru/_static/api/app-info?locale=ru", requestHeaders);
        appInfoStream.Close();

        await Task.Delay(3000);

        var i = 0;
        foreach (var product in category.Data.Products)
        {
            var productItemUrl = string.Format(productUrlFormat, product.Product.ItemId);

            requestHeaders = await GetRequestHeadersAsync(_requestHeadersPath);

            try
            {
                using var redirectStream = await _webLoader.LoadFromUrl("https://goldapple.ru/web/api/v3/catalog/redirect?locale=ru", requestHeaders,
               HttpMethod.Post,
               new { url = product.Product.Url });
                var redirectJson = await JsonSerializer.DeserializeAsync<JsonObject>(redirectStream);
                redirectStream.Close();

                await Task.Delay(1000);

                requestHeaders = await GetRequestHeadersAsync(_requestHeadersPath);
                using var productResponse = await _webLoader.LoadFromUrl(productItemUrl, requestHeaders);
                productResponse.Close();

                i++;

                await Task.Delay(500);

                
            }
            catch
            {
                _testOutputHelper.WriteLine($"try {i + 1} url {productItemUrl}");

                throw;
            }
        }
    }

    [Fact]
    public async Task LoadGoldAppleProductPageTestAsync()
    {
        await InitWebLoaderIfNeedAsync();

        var data = await _browserServiceMock.Object.GetData("goldapple.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidDataException(data.GetType().Name);

        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersPath, cookies);
        
        var stream = await _webLoader.LoadFromUrl(_productUrl, requestHeaders);
        var productData = await JsonSerializer.DeserializeAsync<ProductData>(stream);
        stream.Close();

        Assert.NotNull(productData);
        Assert.NotNull(productData.Data.Name);
        Assert.NotNull(productData.Data.ItemId);
        Assert.NotNull(productData.Data.ProductType);
    }
}