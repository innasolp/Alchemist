using Alchemist.Import.Products.Interfaces;
using BrowserDataLoader.Interfaces;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using WebLoader.Common;
using WebLoader.Interfaces;
using Xunit.Abstractions;

namespace Alchemist.Product.Shop.Ozon.ImportService.Firefox.Tests;

public class OzonImportServiceFirefoxLoaderTest
{
    private readonly ILogger<OzonImportService> _logger = Moq.Mock.Of<ILogger<OzonImportService>>();
    private readonly Moq.Mock<IProductShopModel> _shopUrlModelMock = new();
    private readonly IBrowserDataLoader _dataLoader = new BrowserDataLoader.Firefox.Standart.Windows.FirefoxStandartDataLoader();
    private readonly IWebLoader _webLoader;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly OzonImportService _ozonImportService;
    private readonly string _productUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2Fproduct%2Fd-alba-patchi-s-kollagenom-dlya-oblasti-vokrug-glaz-white-truffle-intensive-the-real-eye-patch-68sht-1062342797%2F%3Flayout_container%3DpdpPage2column%26layout_page_index%3D2%26sh%3DT6fQeg2vGw%26start_page_id%3Dd65cebfe071458bee7a2c2c9494e2f9d";
    private readonly string _categoryUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2Fcategory%2Fantivozrastnoy-uhod-38000%2F%3Flayout_container%3DcategorySearchMegapagination%26layout_page_index%3D2%26page%3D2";
    private readonly string _requestHeadersFireFoxFileName = "Ozon.Headers.Firefox.json";
    private readonly RequestHeaders _requestHeaders;
    private readonly Moq.Mock<IProductItemHandler> _productItemHandler = new(); 
    public OzonImportServiceFirefoxLoaderTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _shopUrlModelMock.Setup(s => s.Categories).Returns(new System.Collections.ObjectModel.ObservableCollection<IProductShopCategoryModel>());
        _webLoader = new WebLoader.Playwright.Firefox.PlaywrightFirefoxLoader(_dataLoader);

        using var s = File.OpenRead($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_requestHeadersFireFoxFileName}");

        _requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(s);

        s.Close();

        //todo
        _ozonImportService = new OzonImportService(_logger, _shopUrlModelMock.Object, _webLoader, _requestHeaders, _productItemHandler.Object);
    }

    private async Task InitializeAsync()
    {
        var cookies = await _dataLoader.LoadCookies();
        Assert.True(cookies.Count > 0);
        Assert.True(cookies.All(c=>c.Value != null));

        var result = await _webLoader.Start();
        Assert.True(result);
    }

    [Fact]
    public async Task LoadOzonProductPageTestAsync()
    {
        if (!_webLoader.IsStarted)
            await InitializeAsync();

        var stream = await _webLoader.LoadFromUrl(_productUrl, _requestHeaders);
        var product = await JsonSerializer.DeserializeAsync<Model.Product>(stream);
        stream.Close();

        Assert.NotNull(product);
        Assert.NotNull(product.WebRichDescription);
        Assert.NotNull(product.Name);
        Assert.NotNull((product as IProductItem)?.ItemId);
        Assert.NotNull((product as IProductItem)?.Country);
    }

    [Fact]
    public async Task LoadOzonCategoryPageTestAsync()
    {
        if (!_webLoader.IsStarted)
            await InitializeAsync();

        var stream = await _webLoader.LoadFromUrl(_categoryUrl, _requestHeaders);
        var category = await JsonSerializer.DeserializeAsync<Model.Category>(stream);
        stream.Close();

        Assert.NotNull(category);
        Assert.NotNull(category.WidgetStates);
        Assert.NotNull(category.CategoryContent);
        Assert.NotNull(category.CategoryContent.Items);
        Assert.NotEmpty(category.CategoryContent.Items);
        Assert.DoesNotContain(category.CategoryContent.Items, i => string.IsNullOrEmpty((i as ICategoryProductItem)?.Id));
    }
}