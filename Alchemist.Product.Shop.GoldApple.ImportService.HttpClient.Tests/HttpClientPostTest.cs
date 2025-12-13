using Alchemist.Product.Shop.GoldApple.Model;
using Alchemist.Test.Product.Shop;
using BrowserDataLoader.Firefox.Standart.Windows;
using BrowserDataLoader.Interfaces;
using BrowserLauncher.Firefox.Windows.Standart;
using BrowserLauncher.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Xunit.Abstractions;

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

    private async Task<Dictionary<string,string>> GetRequestHeadersAsync(string requestHeadersPath)
    {
        var data = await _browserServiceMock.Object.GetData("goldapple.ru");
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidDataException(data.GetType().Name);

        return HeadersHelper.LoadHeadersForRequest(requestHeadersPath, cookies);
    }

    [Fact]
    public async Task LoadCategoryProductsTest()
    {
        var httpClientFactory = GetHttpClientFactory();
        var httpClient = httpClientFactory.CreateClient();
        

        var requestHeadersPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_requestHeadersFileName}";

        var requestHeaders = await GetRequestHeadersAsync(requestHeadersPath);

        var url = "https://goldapple.ru/front/api/catalog/cards-list?locale=ru";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        requestHeaders.ToList().ForEach(h => req.Headers.Add(h.Key, h.Value));
        var content = new
        {
            categoryId = 1000000276,
            mode = "simple",
            pageNumber = 11,
            pageSize = 24,
            cityId = "555e7d61-d9a7-4ba6-9770-6caa8198c483"
        };
        req.Content = JsonContent.Create(content);

        var response = await httpClient.SendAsync(req);

        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();

        var utf8Bytes = DecompressGZipByteArrayToUtf8Bytes(bytes);
        using var utf8Stream = new MemoryStream(utf8Bytes);
        var category = await JsonSerializer.DeserializeAsync<CategoryProducts>(utf8Stream);
        utf8Stream.Close();

        Assert.NotEmpty(category.Data.Products);

        await Task.Delay(2000);

        var productUrlFormat = "https://goldapple.ru/front/api/catalog/product-card/base/v3?itemId={0}&cityId=0c5b2444-70a0-4932-980c-b4dc0d3f02b5&customerGroupId=0";

        var i = 0;
        foreach (var product in category.Data.Products)
        {
            var productItemUrl = string.Format(productUrlFormat, product.Product.ItemId);

            requestHeaders = await GetRequestHeadersAsync(requestHeadersPath);

            using var productResponse = await httpClient.GetAsync(productItemUrl);

            try
            {
                productResponse.EnsureSuccessStatusCode();

                i++;

                await Task.Delay(2000);
            }
            catch
            {
                _testOutputHelper.WriteLine($"try {i+1} url {productItemUrl}");

                throw;
            }
        }
    }

    private static byte[] DecompressGZipByteArrayToUtf8Bytes(byte[] compressedGZipBytes)
    {
        using MemoryStream compressedStream = new(compressedGZipBytes);
        using GZipStream decompressorStream = new(compressedStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();

        decompressorStream.CopyTo(decompressedStream);
        var decompressedBytes = decompressedStream.ToArray();
        return decompressedBytes;
    }
}
