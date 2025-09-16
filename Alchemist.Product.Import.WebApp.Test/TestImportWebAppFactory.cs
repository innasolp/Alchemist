using Alchemist.DataService.Interfaces;
using Alchemist.Product.Import.Model;
using Alchemist.Product.RestAPIClient;
using Alchemist.Settings.RestAPIClient;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Alchemist.Product.Import.WebApp.Test;

public class TestImportWebAppFactory() : TestWebAppKestrelFactory<ImportWebAppProgram>(8110, 8111)
{
    public Mock<IShopDataService> ShopAPIClient { get; } = new Mock<IShopDataService>();

    public Mock<IShopSettingsDataService> SettingsAPIClient { get; } = new Mock<IShopSettingsDataService>();

    private IImportFacade? _importFacade;

    public void Reset()
    {
        EnsureServer();
        _importFacade ??= Host.Services.GetRequiredService<IImportFacade>();
        _importFacade?.Reset();
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);       

        builder.ConfigureTestServices(MockAPIServiceClients);
    }

    private void MockAPIServiceClients(IServiceCollection services)
    {
        services.InterceptImplementation<IShopDataService, ShopApiClient>(ShopAPIClient.Object);

        services.InterceptImplementation<IShopSettingsDataService, SettingsAPIClient>(SettingsAPIClient.Object);
    }
}
