using Alchemist.Test.Product.Shop;
using BrowserDataLoader.Firefox.Standart.Windows;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Firefox.Windows.Standart;
using BrowserLauncher.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Alchemist.Shop.GoldenApple.ImportService.HttpClient.Tests;

public class HttpClientPostTest(ITestOutputHelper testOutputHelper) : ProductShopTest
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    private IHttpClientFactory GetHttpClientFactory()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddHttpClient();
        var host = builder.Build();

        return host.Services.GetRequiredService<IHttpClientFactory>();
    }
    

    private readonly string _requestHeadersFileName = "GoldApple.Headers.Firefox.json";

    private readonly IBrowserDataLoader _browserDataLoader = new FirefoxStandartDataLoader();

    protected override IBrowserDataLoader BrowserDataLoader => _browserDataLoader;

    private readonly IBrowserLauncher _browserLauncher = new FirefoxStandartBrowserLauncher();

    protected override IBrowserLauncher BrowserLauncher => _browserLauncher;

    [Fact]
    public async Task LoadCategoryProductsTest()
    {
        var httpClientFactory = GetHttpClientFactory();
        var httpClient = httpClientFactory.CreateClient();
        
        var requestHeadersPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_requestHeadersFileName}";
        var data = await _browserServiceMock.Object.GetData("goldapple.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidDataException(data.GetType().Name);

        var requestHeaders = HeadersHelper.LoadHeadersForRequest(requestHeadersPath, cookies);

        var url = "https://goldapple.ru/front/api/catalog/cards-list?locale=ru";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        requestHeaders.ToList().ForEach(h => req.Headers.Add(h.Key, h.Value));
        var content = new {
            categoryId = 1000000276,
            mode = "simple",
            pageNumber = 11,
            pageSize = 24,
            cityId = "555e7d61-d9a7-4ba6-9770-6caa8198c483"
        };
        req.Content = JsonContent.Create(content);
        req.RequestUri = new Uri(url);
        var response = await httpClient.SendAsync(req);
        var responseText = await response.Content.ReadAsStringAsync();
        try
        {
            Assert.True(response.IsSuccessStatusCode);
        }
        catch
        {
            _testOutputHelper.WriteLine(responseText);
            throw;
        }
    }
}
