using Alchemist.Import.Settings.Model;
using Json.Extensions;

namespace Alchemist.Product.ImportBackground.Tests;

public class BuildImportServicesFromJsonTest:BuildImportServiceTest
{
    private readonly string _shopProductsJsonFile = "shopProducts.json";
    private readonly string _shopCategoriesJsonFile = "shopCategories.json";    

    protected override async Task SetShopSettings()
    {
        _shopCategoriesSettings = await _shopCategoriesJsonFile.ReadFromJsonFileAsync<CategoryShopImportSettings[]>();// "ShopCategories");
        _shopProductsSettings = await _shopProductsJsonFile.ReadFromJsonFileAsync<ProductShopImportSettings[]>();// "ShopProducts");        
    }
}