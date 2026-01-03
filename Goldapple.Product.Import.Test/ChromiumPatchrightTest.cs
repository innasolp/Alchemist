using WebLoader.Playwright.ChromiumPatchright;
using Xunit.Abstractions;

namespace Goldapple.Product.Import.Test;

public class ChromiumPatchrightTest(ITestOutputHelper testOutputHelper)
{
    private readonly string _categoryApiUrl = "https://goldapple.ru/front/api/catalog/cards-list";
    private readonly string _productApiUrl = "https://goldapple.ru/front/api/catalog/product-card/base";

    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
    

    [Fact]
    public async Task LoadGoldAppleCategoryPageTestAsync()
    {
        var categoryPageUrl = "https://goldapple.ru/makijazh/glaza/glitter";

        await using var webLoader = new ChromiumPatchrightWebLoader();

        await webLoader.Start();
        var (success, stream) = await webLoader.TryLoadFromRoute(categoryPageUrl, (routeUrl)=> routeUrl.Contains(_categoryApiUrl));
         
        Assert.True(success);

        stream.Close();

        await webLoader.Close();
    }

    [Fact]
    public async Task LoadGoldAppleProductPageTestAsync()
    {
        var productPageUrl = "https://goldapple.ru/19000249545-clarifying";

        await using var webLoader = new ChromiumPatchrightWebLoader();

        await webLoader.Start();

        var (success, stream) = await webLoader.TryLoadFromRoute(productPageUrl, (routeUrl)=>routeUrl.Contains(_productApiUrl));

        Assert.True(success);

        stream.Close();

        await webLoader.Close();
    }
}