using Alchemist.Import.Settings.Model;
using Json.Extensions;
using System.Text.Json;
using WebLoader.Common;

namespace Alchemist.Import.ProductSettings.Tests;

public class SerializationTests
{
    private readonly string _productsJsonFileName = "shopProducts.json";

    [Fact]
    public async Task TestLoadProductSettings()
    {
        var shopProductsSettings = await _productsJsonFileName.ReadFromJsonFileAsync<ShopImportSettings[]>();

        Assert.NotNull(shopProductsSettings);
        Assert.Equal(2, shopProductsSettings.Length);

        var ozonSettings = shopProductsSettings.FirstOrDefault(s => s.Name == "Ozon");
        Assert.NotNull(ozonSettings);
        Assert.NotNull(ozonSettings.RequestHeaders);
        Assert.NotNull(ozonSettings.RequestHeaders.Value);
        Assert.NotNull(ozonSettings.WebLoader.AssemblyPath);
        Assert.NotNull(ozonSettings.WebLoader.ImplementationTypeName);

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(ozonSettings.RequestHeaders.Value);
        Assert.NotNull(requestHeaders);
        Assert.NotNull(requestHeaders.Headers);


        var goldAppleSettings = shopProductsSettings.FirstOrDefault(s => s.Name == "GoldApple");
        Assert.NotNull(goldAppleSettings);
        Assert.Null(goldAppleSettings.RequestHeaders);
        Assert.NotNull(goldAppleSettings.WebLoader.AssemblyPath);
        Assert.NotNull(goldAppleSettings.WebLoader.ImplementationTypeName);
    }
}