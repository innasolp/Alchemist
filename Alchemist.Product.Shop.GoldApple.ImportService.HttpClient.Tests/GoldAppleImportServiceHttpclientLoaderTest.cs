using Microsoft.Extensions.Logging;
using WebLoader.HttpClient;
using WebLoader.Interfaces;
using Xunit.Abstractions;
using System.Text.Json;
using Alchemist.Product.Shop.GoldApple.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.Product.Interfaces;
using Alchemist.Product.Shop.GoldApple.ImportService;
using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Shop.GoldenApple.ImportService.HttpClient.Tests;

public class GoldAppleImportServiceHttpclientLoaderTest
{
    private readonly string _categoryUrl = "https://goldapple.ru/front/api/catalog/products?categoryId=1000000252&cityId=555e7d61-d9a7-4ba6-9770-6caa8198c483&cityDistrict=%D0%9C%D0%BE%D1%81%D0%BA%D0%BE%D0%B2%D1%81%D0%BA%D0%B8%D0%B9&geoPolygons[]=EKB-000000288&geoPolygons[]=EKB-000000749&pageNumber=3";
    private readonly string _productUrl = "https://goldapple.ru/front/api/catalog/product-card/base?itemId=99730300001&cityId=0c5b2444-70a0-4932-980c-b4dc0d3f02b5&customerGroupId=0";

    private readonly ILogger<GoldAppleImportService> _logger = Moq.Mock.Of<ILogger<GoldAppleImportService>>();
    private readonly Moq.Mock<IProductShopModel> _shopUrlModelMock = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebLoader _webLoader;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly GoldAppleImportService _importService;

    public GoldAppleImportServiceHttpclientLoaderTest(ITestOutputHelper testOutputHelper)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddHttpClient();
        var host = builder.Build();
        _httpClientFactory = host.Services.GetService<IHttpClientFactory>();

        _shopUrlModelMock.Setup(s => s.Categories).Returns(new System.Collections.ObjectModel.ObservableCollection<IShopCategory>());

        _testOutputHelper = testOutputHelper;
        _webLoader = new HttpClientWebLoader(_httpClientFactory);
        _importService = new GoldAppleImportService(_logger, _shopUrlModelMock.Object, _webLoader);
    }

    private async Task InitializeAsync()
    {
        var result = await _webLoader.Start(_importService.RequestHeaders);
        Assert.True(result);
    }

    [Fact]
    public async Task LoadGoldAppleCategoryPageTestAsync()
    {
        if (!_webLoader.IsStarted)
            await InitializeAsync();

        var stream = await _webLoader.LoadFromUrl(_categoryUrl);
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
        if (!_webLoader.IsStarted)
            await InitializeAsync();

        var stream = await _webLoader.LoadFromUrl(_productUrl);
        var productData = await JsonSerializer.DeserializeAsync<ProductData>(stream);
        stream.Close();

        Assert.NotNull(productData);
        Assert.NotNull(productData.Data.Name);
        Assert.NotNull(productData.Data.ItemId);
        Assert.NotNull(productData.Data.ProductType);
    }
}