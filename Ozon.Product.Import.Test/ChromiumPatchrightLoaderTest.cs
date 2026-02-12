using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using Product.Import.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;
using WebLoader.Playwright.ChromiumPatchright;
using WebLoader.Playwright.Firefox;
using Xunit.Abstractions;

namespace Ozon.Product.Import.Test;

public class ChromiumPatchrightLoaderTest(ITestOutputHelper testOutputHelper) 
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    private readonly string _productApiUrlFormat = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2Fproduct%2F{0}";
    private readonly string _categoryApiUrlFormat = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2Fcategory%2F{0}";

    private readonly string _productItem = "d-alba-patchi-s-kollagenom-dlya-oblasti-vokrug-glaz-white-truffle-intensive-the-real-eye-patch-68sht-1062342797";
    private readonly string _productPathFormat = "/product/{0}";
    private readonly string _categoryPathFormat = "/category/{0}";
    private readonly string _categoryItem = "antivozrastnoy-uhod-38000";

    private readonly string _shopUrlFormat = "https://www.ozon.ru{0}";

    [Fact]
    public async Task LoadOzonProductPageTestAsync()
    {       
        await using var webLoader = new ChromiumPatchrightWebLoader();

        await webLoader.Start();

        var (success, stream) = await webLoader.TryLoadFromRouteAsync(string.Format(_shopUrlFormat, string.Format(_productPathFormat, _productItem)), 
            url=>url.Contains(string.Format(_productApiUrlFormat, _productItem)),
            loadingType: WebLoader.Playwright.RouteType.Request,
            timeoutInMilliseconds:5000);

        using (stream)

            Assert.True(success);

            stream?.Close();

        await webLoader.Close();
    }

    [Fact]
    public async Task LoadOzonCategoryPageTestAsync()
    {
        await using var webLoader = new ChromiumPatchrightWebLoader();

        await webLoader.Start();

        var (success, stream) = await webLoader.TryLoadFromRouteAsync(string.Format(_shopUrlFormat, string.Format(_categoryPathFormat, _categoryItem)),
            url => url.Contains(string.Format(_categoryApiUrlFormat, _categoryItem)),
            loadingType: WebLoader.Playwright.RouteType.Request,
            timeoutInMilliseconds: 5000);

        using (stream)

            Assert.True(success);

            stream?.Close();

        await webLoader.Close();
    }

    [Fact]
    public async Task LoadOzonProductFromApiAfterLoadingStartPageAsync()
    {
        var routeUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2FsearchSuggestions";

        await using var webLoader = new ChromiumPatchrightWebLoader();

        await webLoader.Start();

        await webLoader.WaitForUrlAsync("https://www.ozon.ru", (url) => url.Contains(routeUrl), timeoutInMilliseconds: 10000);

        using var stream = await webLoader.LoadFromApiRequestAsync($"{string.Format(_productApiUrlFormat, _productItem)}%2F%3Flayout_container%3DpdpPage2column%26layout_page_index%3D2", 
            timeoutInMilliseconds: 5000);

        Assert.NotNull(stream);

        var json = await JsonSerializer.DeserializeAsync<JsonObject>(stream);

        stream?.Close();

        await webLoader.Close();

        Assert.Contains("webDescription", json?.ToJsonString());
    }

    [Fact]
    public async Task LoadOzonCategoryFromApiAfterLoadingStartPageAsync()
    {
        var routeUrl = "https://www.ozon.ru/api/entrypoint-api.bx/page/json/v2?url=%2FsearchSuggestions";

        await using var webLoader = new ChromiumPatchrightWebLoader();

        await webLoader.Start();

        await webLoader.WaitForUrlAsync("https://www.ozon.ru", (url) => url.Contains(routeUrl), timeoutInMilliseconds: 10000);

        using var stream = await webLoader.LoadFromApiRequestAsync($"{string.Format(_categoryApiUrlFormat, _categoryItem)}%2F%3Flayout_page_index%3D2", 
            timeoutInMilliseconds: 5000);

        Assert.NotNull(stream);

        var json = await JsonSerializer.DeserializeAsync<JsonObject>(stream);

        stream?.Close();

        await webLoader.Close();

        Assert.Contains("tileGridDesktop", json?.ToJsonString());
        Assert.Contains("items", json?.ToJsonString());
    }
}