using Alchemist.DataService.Interfaces;
using Alchemist.Product.RestAPIClient;
using Alchemist.Settings.RestAPIClient;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Alchemist.Product.Import.WebApp.Test;

public class TestImportWebAppFactory()
    : TestWebAppFactory<ImportWebAppProgram>
{
    public Mock<IShopDataService> ShopAPIClient { get; } = new Mock<IShopDataService>();

    public Mock<IShopSettingsDataService> SettingsAPIClient { get; } = new Mock<IShopSettingsDataService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices((context, services) =>
        {
            var http = context.Configuration.GetSection("Kestrel:EndPoints:Http:Url");
            http.Value = $"https://localhost:{8110}";
            var https = context.Configuration.GetSection("Kestrel:EndPoints:Https:Url");
            https.Value = $"https://localhost:{8111}";
        });

        builder.ConfigureTestServices(MockAPIServiceClients);
    }

    private void MockAPIServiceClients(IServiceCollection services)
    {
        var shopAPIClientDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IShopDataService) && s.ImplementationType == typeof(ShopApiClient));
        if (shopAPIClientDescriptor != null)
            services.Remove(shopAPIClientDescriptor);

        services.AddSingleton(ShopAPIClient.Object);

        var settingsAPIClientDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IShopSettingsDataService) && s.ImplementationType == typeof(SettingsAPIClient));
        if (settingsAPIClientDescriptor != null)
            services.Remove(settingsAPIClientDescriptor);

        services.AddSingleton(SettingsAPIClient.Object);
    }
}
