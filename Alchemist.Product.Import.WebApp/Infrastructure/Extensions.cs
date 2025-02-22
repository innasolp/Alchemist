using Alchemist.Product.Import.WebApp.Models;

namespace Alchemist.Product.Import.WebApp.Infrastructure;

public static class Extensions
{
    public static ServiceSettingsModel? GetServiceSettings(this ShopSettingsModel shopSettings, string serviceName)
    {
        switch (serviceName)
        {
            case nameof(ShopSettingsModel.ImportService):
                return shopSettings.ImportService;

            case nameof(ShopSettingsModel.WebLoader):
                return shopSettings.WebLoader;

            case nameof(ShopSettingsModel.BrowserDataLoader):
                return shopSettings.BrowserDataLoader;

            default:
                return null;
        }
    }

    [Obsolete("Update with serviceName parameter")]
    public static bool UpdateServiceSettings(this ShopSettingsModel shopSettings, ServiceSettingsModel? serviceSettings)
    {
        if (serviceSettings == null) return false;
        switch (serviceSettings?.ServiceName)
        {
            case nameof(ShopSettingsModel.ImportService):
                {
                    shopSettings.ImportService ??= new ServiceSettingsModel { ShopId = shopSettings.ShopId, ShopSettingsId = shopSettings.Id };
                    shopSettings.ImportService.Update(serviceSettings);

                    if (!shopSettings.ServiceSettings.Any(s => s.Guid == serviceSettings?.Guid))
                        shopSettings.ServiceSettings.Add(shopSettings.ImportService);

                    return true;
                }

            case nameof(ShopSettingsModel.WebLoader):
                {
                    shopSettings.WebLoader ??= new ServiceSettingsModel { ShopId = shopSettings.ShopId, ShopSettingsId = shopSettings.Id };
                    shopSettings.WebLoader.Update(serviceSettings);

                    if (!shopSettings.ServiceSettings.Any(s => s.Guid == serviceSettings?.Guid))
                        shopSettings.ServiceSettings.Add(shopSettings.WebLoader);
                    return true;
                }

            case nameof(ShopSettingsModel.BrowserDataLoader):
                {
                    shopSettings.BrowserDataLoader ??= new ServiceSettingsModel { ShopId = shopSettings.ShopId, ShopSettingsId = shopSettings.Id };
                    shopSettings.BrowserDataLoader.Update(serviceSettings);

                    if (!shopSettings.ServiceSettings.Any(s => s.Guid == serviceSettings?.Guid))
                        shopSettings.ServiceSettings.Add(shopSettings.BrowserDataLoader);

                    return true;
                }

            default:
                return false;
        }
    }

    public static void UpdateSettings(this ShopImportModel shopImport, SettingsModelBase settings)
    {
        switch (settings.Type)
        {
            case SettingsType.Shop:
                shopImport.ShopSettings.Update(settings);
                break;

            case SettingsType.Products:
                shopImport.ImportProducts.Update(settings);
                break;

            case SettingsType.Categories:
                shopImport.ImportCategories.Update(settings);
                break;

            default:
                shopImport.ShopSettings.Update(settings);
                break;

        }
    }
}
