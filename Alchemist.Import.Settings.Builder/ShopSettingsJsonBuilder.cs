using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Model;
using Alchemist.Product.Entities;
using Json.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Import.Settings.Builders;

public class ShopSettingsJsonBuilder(string shopProductsJsonFile, string shopCategoriesJsonFile)
    : ISettingsBuilder
{
    private readonly string _shopProductsJsonFile = shopProductsJsonFile;
    private readonly string _shopCategoriesJsonFile = shopCategoriesJsonFile;

    public async Task<List<IShopImportData>> Build(IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<ShopSettingsJsonBuilder>>();
        try
        {
            var jsonProductObjects = await _shopProductsJsonFile.ReadFromJsonFileAsync<JsonObject[]>();
            var shopCategorySettings = await _shopCategoriesJsonFile.ReadFromJsonFileAsync<CategoryShopImportSettings[]>();

            var shopProductSettings = jsonProductObjects.Select(j => new
            {
                Shop = JsonSerializer.Deserialize<Shop>(j.ToString()),
                ShopUrl = JsonSerializer.Deserialize<ShopUrl>(j.ToString()),
                ProductSettings = JsonSerializer.Deserialize<ProductShopImportSettings>(j.ToString()),
            }
            );

            var result = (from product in shopProductSettings
                          join category in shopCategorySettings on product.Shop?.Name equals category.ShopName into categories

                          from shopCategory in categories.DefaultIfEmpty()

                          where product.Shop != null && !string.IsNullOrEmpty(shopCategory?.ShopName)
                          select new ShopImportData
                          (
                              product.Shop,
                              product.ShopUrl,
                              product.ProductSettings,
                              shopCategory
                          )).OfType<IShopImportData>().ToList();

            logger.LogInformation("Shop import settings built from json.");

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return await Task.FromResult(new List<IShopImportData>());
        }
    }
}
