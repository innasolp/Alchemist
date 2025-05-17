using WebLoader.Interfaces;
using Xunit.Abstractions;
using System.Text.Json;
using Alchemist.Product.Shop.GoldApple.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WebLoader.Common;
using System.Reflection;
using Json.FileExtensions;
using BrowserDataLoader.Interfaces;
using BrowserDataLoader.Firefox.Standart.Windows;
using WebLoader.Playwright.Firefox;

namespace Alchemist.Shop.GoldenApple.ImportService.HttpClient.Tests;

public class GoldAppleImportServiceFirefoxTest
{
    private readonly string _categoryUrl = "https://goldapple.ru/front/api/catalog/products?categoryId=1000000252&cityId=555e7d61-d9a7-4ba6-9770-6caa8198c483&cityDistrict=%D0%9C%D0%BE%D1%81%D0%BA%D0%BE%D0%B2%D1%81%D0%BA%D0%B8%D0%B9&geoPolygons[]=EKB-000000288&geoPolygons[]=EKB-000000749&pageNumber=3";
    private readonly string _productUrl = "https://goldapple.ru/front/api/catalog/product-card/base?itemId=99730300001&cityId=0c5b2444-70a0-4932-980c-b4dc0d3f02b5&customerGroupId=0";

    private readonly IWebLoader _webLoader;
    private readonly ITestOutputHelper _testOutputHelper;
    
    private readonly string _requestHeadersFileName = "GoldApple.Headers.Firefox.json";

    private readonly string _requestHeadersPath;

    private readonly IBrowserDataLoader _browserDataLoader;

    public GoldAppleImportServiceFirefoxTest(ITestOutputHelper testOutputHelper)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddHttpClient();
        var host = builder.Build();        

        _testOutputHelper = testOutputHelper;
        _browserDataLoader = new FirefoxStandartDataLoader();
        _webLoader = new PlaywrightFirefoxLoader(_browserDataLoader);  
        _requestHeadersPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_requestHeadersFileName}";       
    }
    private static RequestHeaders GetRequestHeaders(string requestHeadersFileName)
    {
        using var s = File.OpenRead($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{requestHeadersFileName}");

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(s);

        s.Close();

        if (requestHeaders == null)
            Assert.Fail("request header not loaded");

        return requestHeaders;
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

        var requestHeaders = GetRequestHeaders(_requestHeadersFileName);

        var stream = await _webLoader.LoadFromUrl(_categoryUrl, requestHeaders);
        var category = await JsonSerializer.DeserializeAsync<CategoryProducts>(stream);
        stream.Close();

        Assert.NotNull(category);
        Assert.NotNull(category.Data);
        Assert.True(category.Data.Count > 0);
        Assert.NotEmpty(category.Data.Products);
    }

    [Fact]
    public async Task LoadGoldAppleProductPageTestAsync()
    {
        await InitWebLoaderIfNeedAsync();

        var requestHeaders = GetRequestHeaders(_requestHeadersFileName);

        var stream = await _webLoader.LoadFromUrl(_productUrl, requestHeaders);
        var productData = await JsonSerializer.DeserializeAsync<ProductData>(stream);
        stream.Close();

        Assert.NotNull(productData);
        Assert.NotNull(productData.Data.Name);
        Assert.NotNull(productData.Data.ItemId);
        Assert.NotNull(productData.Data.ProductType);
    }
}