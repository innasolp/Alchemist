using Alchemist.Product.Shop.GoldApple.Model;
using System.Text.Json;
using WebLoader.Playwright.ChromiumPatchright;
using Xunit.Abstractions;

namespace Alchemist.Shop.GoldenApple.ImportService.HttpClient.Tests;

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
        
        var category = await JsonSerializer.DeserializeAsync<CategoryProducts>(stream);
        stream.Close();

        Assert.NotNull(category);
        Assert.NotNull(category.Data);
        Assert.True(category.Data.Count > 0);
        Assert.NotEmpty(category.Data.Products);

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

        var productData = await JsonSerializer.DeserializeAsync<ProductData>(stream);
        stream.Close();

        Assert.NotNull(productData);
        Assert.NotNull(productData.Data.Name);
        Assert.NotNull(productData.Data.ItemId);
        Assert.NotNull(productData.Data.ProductType);

        await webLoader.Close();
    }
}