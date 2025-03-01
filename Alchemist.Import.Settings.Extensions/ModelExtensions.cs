using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using System.Text.Json;

namespace Alchemist.Import.Settings.Extensions;

public static class ModelExtensions
{
    public static T GetImportServiceSettings<T>(this IShopSettings shopSettings)
        where T : IImportServiceSettings, new()
    {
        var serviceSettings = JsonSerializer.Deserialize<T>(shopSettings.JsonValue);
        serviceSettings.Id = shopSettings.Id;
        serviceSettings.ParentSettingsId = shopSettings.ParentSettingsId;
        return serviceSettings;
    }

    public static async Task<T> GetProductShopImportSettings<T, TImportServiceSettings>(this IShopSettings shopSettings,
        Func<int, Task<List<IShopSettings>>> getServiceSettings)
         where T : IProductShopImportSettings, new()
        where TImportServiceSettings : IImportServiceSettings, new()
    {
        var productShopImportSettings = await shopSettings.GetShopImportSettings<T, TImportServiceSettings>(getServiceSettings);
        return await Task.FromResult(productShopImportSettings);
    }

    public static void Update(this IImportServiceSettings serviceSettings, IImportServiceSettings source)
    {
        if (source == null) return;

        if (source.Id != null) serviceSettings.Id = source.Id;
        if (source.ParentSettingsId != null) serviceSettings.ParentSettingsId = source.ParentSettingsId;

        serviceSettings.ServiceProviderPath = source.ServiceProviderPath;
        serviceSettings.AssemblyPath = source.AssemblyPath;
        serviceSettings.ServiceTypeName = source.ServiceTypeName;
        serviceSettings.Name = source.Name;
        serviceSettings.ImplementationTypeName = source.ImplementationTypeName;
        serviceSettings.Value = source.Value;
    }

    public static async  Task<TShopImportSettings> GetShopImportSettings<TShopImportSettings, TImportServiceSettings>(this IShopSettings shopSettings,
        Func<int, Task<List<IShopSettings>>> getServiceSettings)
        where TShopImportSettings : IShopImportSettings, new()
        where TImportServiceSettings : IImportServiceSettings, new()
    {
        var shopImportSettings = JsonSerializer.Deserialize<TShopImportSettings>(shopSettings.JsonValue);

        shopImportSettings.Id = shopSettings.Id;
        shopImportSettings.ShopId = shopSettings.ShopId;

        var services = await getServiceSettings(shopSettings.Id);
        var excludedServices = new List<IImportServiceSettings>();

        shopImportSettings.SetServiceSettingsIfAvailable<TImportServiceSettings>(s => s.ImportService,
            nameof(IShopImportSettings.ImportService),
            services,
            s => { shopImportSettings.ImportService = s; excludedServices.Add(s); } );

        shopImportSettings.SetServiceSettingsIfAvailable<TImportServiceSettings>(s => s.RequestHeaders,
            nameof(IShopImportSettings.RequestHeaders),
            services,
            s => {shopImportSettings.RequestHeaders = s; excludedServices.Add(s); });

        shopImportSettings.SetServiceSettingsIfAvailable<TImportServiceSettings>(s => s.BrowserDataLoader,
            nameof(IShopImportSettings.BrowserDataLoader),
            services,
            s => {shopImportSettings.BrowserDataLoader = s; excludedServices.Add(s); });

        shopImportSettings.SetServiceSettingsIfAvailable<TImportServiceSettings>(s => s.WebLoader,
            nameof(IShopImportSettings.WebLoader),
            services,
            s => {shopImportSettings.WebLoader = s; excludedServices.Add(s); });

        services.Where(s => !excludedServices.Any(i => i.Id == s.Id)).ToList().ForEach(s =>
        {
            shopImportSettings.Services.Add(s.GetImportServiceSettings<TImportServiceSettings>());
        });

        return shopImportSettings;
    }

    public static void SetServiceSettingsIfAvailable<TImportServiceSettings>(this IShopImportSettings shopImportSettings,
        Func<IShopImportSettings, IImportServiceSettings?> get,
        string serviceName,
        IList<IShopSettings> services,
        Action<IImportServiceSettings> setNew)
        where TImportServiceSettings : IImportServiceSettings, new()
    {
        var serviceSettings = get(shopImportSettings);
        if (serviceSettings?.Id == null)
        {
            var service = services.FirstOrDefault(s => s.Name == serviceName);
            if (service == null) return;

            var serviceModel = service.GetImportServiceSettings<TImportServiceSettings>();

            if (serviceSettings != null)
                serviceSettings.Update(serviceModel);
            else
            {
                setNew(serviceModel);                
            }
        }
        else
        {
            var service = services.FirstOrDefault(s => s.Id == serviceSettings.Id);
            if (service != null)
                serviceSettings.Update(service.GetImportServiceSettings<TImportServiceSettings>());
        }
    }
}
