using Alchemist.DataService.Interfaces;
using Alchemist.Product.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;

using SettingsDataAdapterFactory = System.Collections.Generic.IDictionary<Alchemist.Product.Interfaces.ShopSettingType, 
    System.Func<Alchemist.Product.Interfaces.ShopSettingType,
        Alchemist.DataService.Interfaces.IShopSettingsDataService, 
        Alchemist.Import.Settings.DataAdapter.ISettingsDataAdapter>>;

using SettingsDataAdapterFactoryImpl = System.Collections.Generic.Dictionary<Alchemist.Product.Interfaces.ShopSettingType, 
    System.Func<Alchemist.Product.Interfaces.ShopSettingType,
        Alchemist.DataService.Interfaces.IShopSettingsDataService,
        Alchemist.Import.Settings.DataAdapter.ISettingsDataAdapter>>;

namespace Alchemist.Import.Settings.DataAdapter;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddSettingsDataAdapter<TShopImportSettings, TImportServiceSettings>
        (this IServiceCollection services, ShopSettingType shopSettingType)
        where TShopImportSettings : class, IShopImportSettings, IShopSettings
    where TImportServiceSettings : class, IServiceSettings, IShopSettings
    {
        return services.AddSingleton<ISettingsAdapter, SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>>(
            serviceProvider =>
        {
            var dataService = serviceProvider.GetRequiredService<IShopSettingsDataService>();
            return new SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(dataService, shopSettingType);
        });
    }

    public static IServiceCollection AddKeyedSettingsDataAdapter<TShopImportSettings, TImportServiceSettings>
        (this IServiceCollection services, ShopSettingType shopSettingType, object key)
        where TShopImportSettings : class, IShopImportSettings, IShopSettings
    where TImportServiceSettings : class, IServiceSettings, IShopSettings
    {
        return services.AddKeyedSingleton<ISettingsAdapter, SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>>(
            key,
            (serviceProvider, key) =>
            {
                var dataService = serviceProvider.GetRequiredService<IShopSettingsDataService>();
                return new SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(dataService, shopSettingType);
            });
    }

    public static IServiceCollection AddTypedSettingsDataAdapter<TShopImportSettings, TImportServiceSettings>
        (this IServiceCollection services, ShopSettingType shopSettingType)
        where TShopImportSettings : class, IShopImportSettings, IShopSettings
    where TImportServiceSettings : class, IServiceSettings, IShopSettings
    {
        return services.AddSingleton<ISettingsDataAdapter, SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>>(
            serviceProvider =>
            {
                var dataService = serviceProvider.GetRequiredService<IShopSettingsDataService>();
                return new SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(dataService, shopSettingType);
            });
    }

    public static IServiceCollection AddKeyedTypedSettingsDataAdapter<TShopImportSettings, TImportServiceSettings>
        (this IServiceCollection services, ShopSettingType shopSettingType, object key)
        where TShopImportSettings : class, IShopImportSettings, IShopSettings
    where TImportServiceSettings : class, IServiceSettings, IShopSettings
    {
        return services.AddKeyedSingleton<ISettingsDataAdapter, SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>>(
            key,
            (serviceProvider, key) =>
            {
                var dataService = serviceProvider.GetRequiredService<IShopSettingsDataService>();
                return new SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(dataService, shopSettingType);
            });
    }

    public static IServiceCollection AddSettingsDataAdapterToCollection<TShopImportSettings, TImportServiceSettings>(this IServiceCollection services,
        ShopSettingType shopSettingType, object key)
        where TShopImportSettings : class, IShopImportSettings, IShopSettings
        where TImportServiceSettings : class, IServiceSettings, IShopSettings
    {
        var factoryDictionaryDescriptor = services.FirstOrDefault(sd => sd.IsKeyedService && sd.ServiceKey == key
            && sd.ServiceType == typeof(SettingsDataAdapterFactory));
        var factories = factoryDictionaryDescriptor?.KeyedImplementationInstance as SettingsDataAdapterFactory
            ?? new SettingsDataAdapterFactoryImpl();
        if (factoryDictionaryDescriptor == null)
            services.AddKeyedSingleton(key, factories);
        
        factories.Add(shopSettingType, (shopSettingType, settingsDataService) => 
                        new SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(settingsDataService, shopSettingType));

        var adapterDictionaryDescriptor = services.FirstOrDefault(sd => sd.IsKeyedService && sd.ServiceKey == key
            && sd.ServiceType == typeof(IDictionary<ShopSettingType, ISettingsDataAdapter>));
        if (adapterDictionaryDescriptor == null)
            services.AddKeyedSingleton<IDictionary<ShopSettingType, ISettingsDataAdapter>>(key, (serviceProvider, key)=>
            {
                var settingsDataService = serviceProvider.GetRequiredService<IShopSettingsDataService>();
                var implFactories = serviceProvider.GetRequiredKeyedService<SettingsDataAdapterFactory> (key);
                var dictionary = new Dictionary<ShopSettingType, ISettingsDataAdapter>();
                foreach (var implFactory in implFactories)
                {
                    dictionary.Add(implFactory.Key, implFactory.Value(implFactory.Key, settingsDataService));
                }
                return dictionary;
            });

        return services;
    }
}
