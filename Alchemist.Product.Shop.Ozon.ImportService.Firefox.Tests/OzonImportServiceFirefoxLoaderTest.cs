using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.Interfaces;
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
    //private readonly string _productUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2Fproduct%2Fd-alba-patchi-s-kollagenom-dlya-oblasti-vokrug-glaz-white-truffle-intensive-the-real-eye-patch-68sht-1062342797%2F%3Fadvert%3DAAwBVa-3AveybX9YFUDfEJ8mibM-6_P1LtAI_oPepci-bfSE3zZdYjC0sCMBWw24dqD4EOvRtZJI3VEGGYjh3LQRYJ5xfl0QSgpAAMkGLhfng5DwvZmM2LgsF5PHPrnFCAmiFKd-i344ym71dpkmqetNFL_DqtFrbm8f0mHZMzEO340z-8et4ibaR0cnXawlcAwFPJwTLc10EgHHqTbPrTaBNVc14-NOsDj-J_EeHnYLGm3YhDj5iSP7-S8eeRtkLXMOm8pb3yh_4AYmj9b7q3woPSqV3ImuMk_f7-vsikoWOvVMwdYyC1mK8b4jm2E6q6xHcBJtZgpYQrF4oafECzqOZE9IL5qBh2ORveqda0kKh8yUJYQjBJopWvzPlM3FYoxNYte6nWM%26avtc%3D1%26avte%3D4%26avts%3D1734067028%26layout_container%3DpdpPage2column%26layout_page_index%3D2%26sh%3DT6fQeg2vGw%26start_page_id%3Dd65cebfe071458bee7a2c2c9494e2f9d";
    private readonly string _productUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2Fproduct%2Fd-alba-patchi-s-kollagenom-dlya-oblasti-vokrug-glaz-white-truffle-intensive-the-real-eye-patch-68sht-1062342797%2F%3Flayout_container%3DpdpPage2column%26layout_page_index%3D2%26sh%3DT6fQeg2vGw%26start_page_id%3Dd65cebfe071458bee7a2c2c9494e2f9d";
    private readonly string _categoryUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2Fcategory%2Fantivozrastnoy-uhod-38000%2F%3Flayout_container%3DcategorySearchMegapagination%26layout_page_index%3D2%26page%3D2";
    private readonly string _requestHeadersFireFoxFileName = "Ozon.Headers.Firefox.json";

    public OzonImportServiceFirefoxLoaderTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _shopUrlModelMock.Setup(s => s.Categories).Returns(new System.Collections.ObjectModel.ObservableCollection<IShopCategory>());
        _webLoader = new WebLoader.Playwright.Firefox.PlaywrightFirefoxLoader(_dataLoader);

        using var s = File.OpenRead($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_requestHeadersFireFoxFileName}");

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(s);

        s.Close();

        _ozonImportService = new OzonImportService(_logger, _shopUrlModelMock.Object, _webLoader, requestHeaders);
    }

    private async Task InitializeAsync()
    {
        var cookies = await _dataLoader.LoadCookies();
        Assert.True(cookies.Count > 0);
        Assert.True(cookies.All(c=>c.Value != null));

        var result = await _webLoader.Start(_ozonImportService.RequestHeaders);
        Assert.True(result);
    }

    [Fact]
    public async Task LoadOzonProductPageTestAsync()
    {
        if (!_webLoader.IsStarted)
            await InitializeAsync();

        var stream = await _webLoader.LoadFromUrl(_productUrl);
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

        var stream = await _webLoader.LoadFromUrl(_categoryUrl);
        var category = await JsonSerializer.DeserializeAsync<Model.Category>(stream);
        stream.Close();

        Assert.NotNull(category);
        Assert.NotNull(category.WidgetStates);
        Assert.NotNull(category.CategoryContent);
        Assert.NotNull(category.CategoryContent.Items);
        Assert.NotEmpty(category.CategoryContent.Items);
    }
}