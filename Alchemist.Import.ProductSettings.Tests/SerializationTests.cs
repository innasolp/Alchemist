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
        var shopProductsSettings = await _productsJsonFileName.ReadFromJsonFileAsync<Dictionary<string,TestProductShopImportSettings>>();

        Assert.NotNull(shopProductsSettings);
        Assert.Equal(2, shopProductsSettings.Count);

        var ozonSettings = shopProductsSettings.FirstOrDefault(s => s.Key == "Ozon").Value;
        Assert.NotNull(ozonSettings);
        Assert.NotNull(ozonSettings.GetRequestHeaders<TestImportServiceSettings>());
        Assert.NotNull(ozonSettings.GetRequestHeaders<TestImportServiceSettings>().Value);
        Assert.NotNull(ozonSettings.GetWebLoader<TestImportServiceSettings>().AssemblyPath);
        Assert.NotNull(ozonSettings.GetWebLoader<TestImportServiceSettings>().ImplementationTypeName);

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(ozonSettings.GetRequestHeaders<TestImportServiceSettings>().Value);
        Assert.NotNull(requestHeaders);
        Assert.NotNull(requestHeaders.Headers);


        var goldAppleSettings = shopProductsSettings.FirstOrDefault(s => s.Key == "GoldApple").Value;
        Assert.NotNull(goldAppleSettings);
        Assert.Null(goldAppleSettings.GetRequestHeaders<TestImportServiceSettings>());
        Assert.NotNull(goldAppleSettings.GetWebLoader<TestImportServiceSettings>().AssemblyPath);
        Assert.NotNull(goldAppleSettings.GetWebLoader<TestImportServiceSettings>().ImplementationTypeName);
    }
}