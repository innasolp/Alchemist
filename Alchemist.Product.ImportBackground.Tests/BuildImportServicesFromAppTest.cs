using Alchemist.Import.Settings.Builders;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Product.ImportBackground.Tests;

public class BuildImportServicesFromAppTest : BuildImportServiceTest
{
    protected override async Task SetShopSettings()
    {
        var builder = new HostApplicationBuilder();
        var shopSettingsAppBuilder = new ShopSettingsAppBuilder(builder, "SettingsAPIHost", "RestAPIHost");
        var host = builder.Build();
        var shopSettings = await shopSettingsAppBuilder.Build(host);

        _shopProductsSettings = shopSettings.Select(s => s.ProductShopImportSettings).ToArray();
        _shopCategoriesSettings = shopSettings.Select(s => s.CategoryShopImportSettings).ToArray();
    }
}
