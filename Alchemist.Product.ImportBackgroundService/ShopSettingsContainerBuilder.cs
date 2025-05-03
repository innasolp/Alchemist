using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Builders;
using Alchemist.Import.Settings.Model;
using Alchemist.Log.Serilog;
using Alchemist.Product.RestAPIClient;
using Alchemist.Settings.RestAPIClient;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Settings.Adapter;

namespace Alchemist.Product.Import.Background.Service;

public static class ShopSettingsContainerBuilder
{
    public static async Task<List<ShopSettingsContainer>> BuildAsync(string[] args, IConfiguration configuration, string logPath, string shopProductsJsonFile, string shopCategoriesJsonFile)
    {
        using var settingsHost = Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(
        settingsWebHostBuilder =>
        {
            settingsWebHostBuilder.UseUrls("http://localhost:9000", "https://localhost:9001");
            settingsWebHostBuilder.UseStartup<SettingsStartup>();

            settingsWebHostBuilder.ConfigureServices((settingsContext, services) =>
            {
                services.ConfigureSettingsBuilders(configuration, shopProductsJsonFile, shopCategoriesJsonFile);
            });
        }
        )
        .ConfigureLogging((settingsHostBuilderContext, settingsLogging) =>
        {
            settingsLogging.BuildSerilog(configuration, settingsHostBuilderContext.HostingEnvironment, logPath);
        })
        .Build();

        var settingsBuilders = settingsHost.Services.GetServices<ISettingsBuilder>().OrderBy(b => b.Priority).ToList();
        var allShopSettings = new List<ShopSettingsContainer>();
        foreach (var settingsBuilder in settingsBuilders)
        {
            allShopSettings = await settingsBuilder.Build();
            if (allShopSettings.Count != 0 && allShopSettings.Any(s => s.ProductShopImportSettings != null || s.CategoryShopImportSettings != null))
                break;
        }

        await settingsHost.StartAsync();
        await settingsHost.StopAsync();

        return allShopSettings;
    }

    private static void BuildSerilog(this ILoggingBuilder settingsLogging,IConfiguration configuration, IHostEnvironment hostEnvironment,  string logPath)
    {
        var settingsSerilogBuilder = new AppSerilogBuilder(configuration, hostEnvironment);
        settingsSerilogBuilder.AddSourceContextLogConfig($"{logPath}/ImportBackgroundService", nameof(ShopSettingsAppBuilder));
        settingsSerilogBuilder.AddSourceContextLogConfig($"{logPath}/ImportBackgroundService", nameof(ShopSettingsJsonBuilder));
        settingsSerilogBuilder.SetSerilog(settingsLogging);
    }

    private static IServiceCollection ConfigureSettingsBuilders(this IServiceCollection services, IConfiguration configuration, string shopProductsJsonFile, string shopCategoriesJsonFile)
    {
        services.AddRestApiClient<IShopDataService, ShopApiClient>(configuration, "RestAPIHost", nameof(ShopApiClient));
        services.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>(configuration, "SettingsAPIHost", nameof(SettingsAPIClient));
        services.AddSettingsDataAdapter<ProductShopImportSettings, CategoryShopImportSettings, ImportServiceSettings>();

        services.AddSettingsAppBuilder(0);
        services.AddSettingsJsonBuilder(1, shopProductsJsonFile, shopCategoriesJsonFile);

        return services;
    }
}
