using BrowserDataLoader.Firefox.Standart.Windows;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Firefox.Windows.Standart;
using BrowserLauncher.Interfaces;
using Product.Import.Test;
using System.Reflection;
using WebLoader.HttpClient;
using Xunit.Abstractions;

namespace Goldapple.Product.Import.Test;

public class HttpClientTest(ITestOutputHelper testOutputHelper) : ProductShopTest
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    private readonly string _requestHeadersFileName = "GoldApple.Headers.Firefox.json";

    private readonly IBrowserDataLoader _browserDataLoader = new FirefoxStandartDataLoader();

    protected override IBrowserDataLoader BrowserDataLoader => _browserDataLoader;

    private readonly IBrowserLauncher _browserLauncher = new FirefoxStandartBrowserLauncher();

    protected override IBrowserLauncher BrowserLauncher => _browserLauncher;

    private async Task<Dictionary<string,string>> GetRequestHeadersAsync(string requestHeadersPath)
    {
        var data = await BrowserDataLoader.LoadCookies("goldapple.ru");// _browserServiceMock.Object.GetData("goldapple.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidDataException(data.GetType().Name);

        return HeadersHelper.LoadHeadersForRequest(requestHeadersPath, cookies);
    }

    [Fact]
    public async Task LoadCategoryProductsTestAsync()
    {
        await using var webLoader = new HttpClientWebLoader();        

        var requestHeadersPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_requestHeadersFileName}";

        var requestHeaders = await GetRequestHeadersAsync(requestHeadersPath);

        var url = "https://goldapple.ru/front/api/catalog/cards-list?locale=ru";
        
        var content = new
        {
            categoryId = 1000000276,
            mode = "simple",
            pageNumber = 11,
            pageSize = 24,
            cityId = "555e7d61-d9a7-4ba6-9770-6caa8198c483"
        };        

        await webLoader.Start();
        var response = await webLoader.LoadFromUrl(url, requestHeaders, HttpMethod.Post, content);

        Assert.NotNull(response);

        await webLoader.Close();       
    }
}