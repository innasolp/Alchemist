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
        _shopImportData = await shopSettingsAppBuilder.Build(host);
    }
}
