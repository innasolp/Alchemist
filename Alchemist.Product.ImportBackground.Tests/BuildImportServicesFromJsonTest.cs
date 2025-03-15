using Alchemist.Import.Settings.Builders;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Product.ImportBackground.Tests;

public class BuildImportServicesFromJsonTest:BuildImportServiceTest
{
    private readonly string _shopProductsJsonFile = "shopProducts.json";
    private readonly string _shopCategoriesJsonFile = "shopCategories.json";    

    protected override async Task SetShopSettings()
    {
        var shopSettingsJsonBuilder = new ShopSettingsJsonBuilder(_shopProductsJsonFile, _shopCategoriesJsonFile);
        var builder = new HostApplicationBuilder();
        var host = builder.Build();
        _shopImportData = await shopSettingsJsonBuilder.Build(host);
    }
}