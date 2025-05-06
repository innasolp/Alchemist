using Alchemist.Import.Settings.Model;
using Alchemist.Product.Entities;
using Json.FileExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Settings.Builders;

public class ShopSettingsJsonBuilder : ISettingsBuilder
{    
    private readonly string _shopProductsJsonFile;
    private readonly string _shopCategoriesJsonFile;
    private readonly ILogger<ShopSettingsJsonBuilder> _logger;

    internal ShopSettingsJsonBuilder(ILogger<ShopSettingsJsonBuilder> logger, 
        int priority,
        [FromKeyedServices($"{nameof(ShopSettingsJsonBuilder)}Products")]string shopProductsJsonFile,
        [FromKeyedServices($"{nameof(ShopSettingsJsonBuilder)}Categories")] string shopCategoriesJsonFile)
    {
        _shopProductsJsonFile = shopProductsJsonFile;
        _shopCategoriesJsonFile = shopCategoriesJsonFile;
        _logger = logger;
        Priority = priority;
    }

    public int Priority { get; }

    public async Task<List<ShopSettingsContainer>> Build()
    {
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

            _logger.LogInformation("Shop import settings built from json.");

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return await Task.FromResult(new List<ShopSettingsContainer>());
        }
    }
}
