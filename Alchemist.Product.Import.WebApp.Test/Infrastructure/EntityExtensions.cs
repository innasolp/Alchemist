using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Settings.Extensions;

namespace Alchemist.Product.Import.WebApp.Test.Infrastructure;

public static class EntityExtensions
{
    public static void SaveService(this List<IShopSettings> shopSettings, IShopSettings serviceSettings)
    {
        if (serviceSettings.Id == 0)
        {
            serviceSettings.Id = shopSettings.Count + 1;
            shopSettings.Add(serviceSettings);
        }
        else
        {
            var existingItem = shopSettings.FirstOrDefault(s => s.Id == serviceSettings.Id);
            var index = shopSettings.IndexOf(existingItem);
            shopSettings[index] = serviceSettings;
        }
    }
    public static async Task<ShopSettingsModel> GetShopImportSettingsAsync(this IShopSettings shopSettings, IEnumerable<IShopSettings> children)
    {
        return shopSettings.Type == ShopSettingType.Product
             ? await shopSettings.GetShopImportSettings<ProductShopSettingsModel, ServiceSettingsModel>(children)
            : await shopSettings.GetShopImportSettings<CategoryShopSettingsModel, ServiceSettingsModel>(children);

    }

}
