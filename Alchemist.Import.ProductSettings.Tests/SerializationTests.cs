using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Test.Model;
using Json.FileExtensions;
using System.Text.Json;
using WebLoader.Common;

namespace Alchemist.Import.ProductSettings.Tests;

public class SerializationTests
{
    private readonly string _productsJsonFileName = "shopProducts.json";

    [Fact]
    public async Task TestLoadProductSettings()
    {
        var shopProductsSettings = await _productsJsonFileName.ReadFromJsonFileAsync<TestProductShopImportSettings[]>();

        Assert.NotNull(shopProductsSettings);
        Assert.Equal(2, shopProductsSettings.Length);

        var ozonSettings = shopProductsSettings.FirstOrDefault(s => s.Name == "Ozon");
        Assert.NotNull(ozonSettings);
        Assert.NotNull(ozonSettings.GetRequestHeaders());
        Assert.NotNull(ozonSettings.GetRequestHeaders().Value);
        Assert.NotNull(ozonSettings.GetWebLoader().AssemblyPath);
        Assert.NotNull(ozonSettings.GetWebLoader().ImplementationTypeName);

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(ozonSettings.GetRequestHeaders().Value);
        Assert.NotNull(requestHeaders);
        Assert.NotNull(requestHeaders.Headers);


        var goldAppleSettings = shopProductsSettings.FirstOrDefault(s => s.Name == "GoldApple");
        Assert.NotNull(goldAppleSettings);
        Assert.Null(goldAppleSettings.GetRequestHeaders);
        Assert.NotNull(goldAppleSettings.GetWebLoader().AssemblyPath);
        Assert.NotNull(goldAppleSettings.GetWebLoader().ImplementationTypeName);
    }
}