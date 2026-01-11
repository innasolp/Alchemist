using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using Product.Import.Test;
using WebLoader.Playwright.Firefox;
using Xunit.Abstractions;

namespace Ozon.Product.Import.Test;

public class PlaywrightFirefoxLoaderTest(ITestOutputHelper testOutputHelper) : ProductShopTest
{
    private readonly IBrowserDataLoader _dataLoader = new BrowserDataLoader.Firefox.Standart.Windows.FirefoxStandartDataLoader();
    private readonly IBrowserLauncher _launcher = new BrowserLauncher.Firefox.Windows.Standart.FirefoxStandartBrowserLauncher();
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
    private readonly string _productUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=/product/d-alba-patchi-s-kollagenom-dlya-oblasti-vokrug-glaz-white-truffle-intensive-the-real-eye-patch-68sht-1062342797/?layout_container=pdpPage2column&layout_page_index=2";
    private readonly string _categoryUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2Fcategory%2Fantivozrastnoy-uhod-38000%2F%3Flayout_page_index%3D2%26page%3D2";
    private readonly string _requestHeadersFireFoxFileName = "Ozon.Headers.Firefox.json";

    protected override IBrowserDataLoader BrowserDataLoader => _dataLoader;

    protected override IBrowserLauncher BrowserLauncher => _launcher;

    [Fact]
    public async Task LoadOzonProductPageTestAsync()
    {       
        var data = await BrowserDataLoader.LoadCookies("ozon.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidOperationException(data.GetType().Name);       

        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersFireFoxFileName, cookies);

        await using var webLoader = new PlaywrightFirefoxLoader();

        await webLoader.Start();

        var stream = await webLoader.LoadFromApiRequestAsync(_productUrl, headers: requestHeaders);
        stream.Close();

        await webLoader.Close();
    }

    [Fact]
    public async Task LoadOzonCategoryPageTestAsync()
    {
        var data = await BrowserDataLoader.LoadCookies("ozon.ru"); 
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidOperationException(data.GetType().Name);

        var requestHeaders = HeadersHelper.LoadHeadersForRequest(_requestHeadersFireFoxFileName, cookies);

        await using var webLoader = new PlaywrightFirefoxLoader();

        await webLoader.Start();

        var stream = await webLoader.LoadFromApiRequestAsync(_categoryUrl, headers: requestHeaders);
        stream.Close();

        await webLoader.Close();
    }
}