using Alchemist.Product.Interfaces;

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
}
