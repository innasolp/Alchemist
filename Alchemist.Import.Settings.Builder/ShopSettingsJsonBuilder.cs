using Alchemist.Import.Settings.Model;
using Alchemist.Product.Entities;
using Json.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Settings.Builders;

public class ShopSettingsJsonBuilder(string shopProductsJsonFile, string shopCategoriesJsonFile)
    : ISettingsBuilder
{    
    private readonly string _shopProductsJsonFile = shopProductsJsonFile;
    private readonly string _shopCategoriesJsonFile = shopCategoriesJsonFile;  

    public async Task<List<ShopSettingsContainer>> Build(IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<ShopSettingsJsonBuilder>>();
        try
        {
            var shopProductsSettings = await _shopProductsJsonFile.ReadFromJsonFileAsync<ProductShopImportSettings[]>();
            var shopCategoriesSettings = await _shopCategoriesJsonFile.ReadFromJsonFileAsync<CategoryShopImportSettings[]>();

            var result = (from product in shopProductsSettings
                    join category in shopCategoriesSettings on product.Url equals category.Url into categories
                    from shopCategory in categories.DefaultIfEmpty()
                    select new ShopSettingsContainer
                    (
                        new Shop
                        {
                            Name = product.Name,
                            Url = product.Url
                        },
                        product,
                        shopCategory
                    )).ToList();

            logger.LogInformation("Shop import settings built from json.");

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return await Task.FromResult(new List<ShopSettingsContainer>());
        }
    }
}
