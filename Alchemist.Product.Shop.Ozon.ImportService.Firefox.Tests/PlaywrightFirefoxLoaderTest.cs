using Alchemist.Import.Products.Interfaces;
using Alchemist.Test.Product.Shop;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using System.Text.Json;
using WebLoader.Playwright.Firefox;
using Xunit.Abstractions;

namespace Alchemist.Product.Shop.Ozon.ImportService.Firefox.Tests;

public class PlaywrightFirefoxLoaderTest(ITestOutputHelper testOutputHelper) : ProductShopTest
{
    private readonly IBrowserDataLoader _dataLoader = new BrowserDataLoader.Firefox.Standart.Windows.FirefoxStandartDataLoader();
    private readonly IBrowserLauncher _launcher = new BrowserLauncher.Firefox.Windows.Standart.FirefoxStandartBrowserLauncher();
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
    private readonly string _productUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=/product/d-alba-patchi-s-kollagenom-dlya-oblasti-vokrug-glaz-white-truffle-intensive-the-real-eye-patch-68sht-1062342797/?layout_container=pdpPage2column&layout_page_index=2&sh=6XZBovdrpw";
    private readonly string _categoryUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2Fcategory%2Fantivozrastnoy-uhod-38000%2F%3Flayout_page_index%3D2%26page%3D2";
    private readonly string _requestHeadersFireFoxFileName = "Ozon.Headers.Firefox.json";

    protected override IBrowserDataLoader BrowserDataLoader => _dataLoader;

    protected override IBrowserLauncher BrowserLauncher => _launcher;

    private async Task LoadCookiesAsync()
    {
        var cookies = await _dataLoader.LoadCookies("www.ozon.ru");
        Assert.True(cookies.Count() > 0);
        Assert.True(cookies.All(c=>c.Value != null));
    }

    [Fact]
    public async Task LoadOzonProductPageTestAsync()
    {       
        await LoadCookiesAsync();

        var data = await _browserServiceMock.Object.GetData("ozon.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidOperationException(data.GetType().Name);       

        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersFireFoxFileName, cookies);

        var webLoader = new PlaywrightFirefoxLoader();

        await webLoader.Start();

        var stream = await webLoader.LoadFromApiRequestAsync(_productUrl, headers: requestHeaders);
        var product = await JsonSerializer.DeserializeAsync<Model.Product>(stream);
        stream.Close();

        Assert.NotNull(product);
        Assert.NotNull(product.WebRichDescription);
        Assert.NotNull(product.Name);
        Assert.NotNull((product as IProductItem)?.ItemId);
        Assert.NotNull((product as IProductItem)?.Country);

        await webLoader.Close();
    }

    [Fact]
    public async Task LoadOzonCategoryPageTestAsync()
    {
        await LoadCookiesAsync();

        var data = await _browserServiceMock.Object.GetData("ozon.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidOperationException(data.GetType().Name);

        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersFireFoxFileName, cookies);

        var webLoader = new PlaywrightFirefoxLoader();

        await webLoader.Start();

        var stream = await webLoader.LoadFromApiRequestAsync(_categoryUrl, headers: requestHeaders);
        var category = await JsonSerializer.DeserializeAsync<Model.Category>(stream);
        stream.Close();

        Assert.NotNull(category);
        Assert.NotNull(category.WidgetStates);
        Assert.NotNull(category.CategoryContent);
        Assert.NotNull(category.CategoryContent.Items);
        Assert.NotEmpty(category.CategoryContent.Items);
        Assert.DoesNotContain(category.CategoryContent.Items, i => string.IsNullOrEmpty((i as ICategoryProductItem)?.Id));

        await webLoader.Close();
    }
}