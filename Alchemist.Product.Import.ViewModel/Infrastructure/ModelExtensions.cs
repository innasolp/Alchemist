using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ModelExtensions
{
    public static SettingsModelBase? GetSettings(this ShopImportModel shopImport, TabType tab, bool lastSelected = false)
    {
        return tab switch
        {
            TabType.Shop => !lastSelected ? shopImport.ShopSettingTabs
                                    : shopImport.ShopSettingTabs?.GetShopSettingsByType(shopImport.ShopSettingTabs.SelectedSettingsTab),
            TabType.Products => shopImport.ImportProducts,
            TabType.Categories => shopImport.ImportCategories,
            _ => throw new InvalidOperationException($"No tab type with value {tab}"),
        };
    }

    public static ServiceSettingsModel CreateServiceSettingsModel(this Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName)
    {
        return new ServiceSettingsModel
        {
            ShopGuid = shopGuid,
            ShopSettingsGuid = shopSettingsGuid,
            Name = serviceSettingsName
        };
    }

    public static ServiceSettingsModel CreateServiceSettingsModel(this ShopSettingsModel shopSettings, string serviceSettingsName)
    {
        return shopSettings.ShopGuid.CreateServiceSettingsModel(shopSettings.Guid, serviceSettingsName);
    }

    public static SettingsModelBase? CreateSettings(this ShopImportModel shopImport, TabType tab)
    {
        return tab switch
        {
            TabType.Shop => new ShopSettingTabsModel() { ShopGuid = shopImport.ShopGuid },
            TabType.Products => new ProductsImportSettingsModel() { ShopGuid = shopImport.ShopGuid },
            TabType.Categories => new CategoriesImportSettingsModel() { ShopGuid = shopImport.ShopGuid },
            _ => throw new InvalidOperationException($"No tab type with value {tab}"),
        };
    }

    public static SettingsModelBase? CreateDefaultTabModel(this TabType tab)
    {
        return tab switch
        {
            TabType.Shop => new ShopSettingTabsModel(),
            TabType.Products => new ProductsImportSettingsModel(),
            TabType.Categories => new CategoriesImportSettingsModel(),
            _ => throw new InvalidOperationException($"No tab type with value {tab}"),
        };
    }

    public static ShopSettingsModel CreateShopSettings(this Guid shopGuid, ShopSettingType settingType)
    {
        return settingType == ShopSettingType.Product
           ? new ProductShopSettingsModel { ShopGuid = shopGuid }
           : (settingType == ShopSettingType.Category ? new CategoryShopSettingsModel { ShopGuid = shopGuid }
           : throw new InvalidOperationException($"{settingType}"));
    }

    public static bool SetSettings(this ShopImportModel shopImport, TabType tab, SettingsModelBase settings)
    {
        switch (tab)
        {
            case TabType.Shop:
                {
                    if (settings is ShopSettingTabsModel shopSettingsTabModel)
                    {
                        shopImport.ShopSettingTabs = shopSettingsTabModel;
                        return true;
                    }
                    else
                    {
                        shopImport.ShopSettingTabs ??= new ShopSettingTabsModel() { ShopGuid = shopImport.ShopGuid };
                        if (settings is ProductShopSettingsModel productShopSettings)
                        {                            
                            shopImport.ShopSettingTabs.ShopProductsSettings = productShopSettings;
                            shopImport.ShopSettingTabs.ShopProductsSettings.Init(shopImport.ShopGuid);
                            return true;
                        }
                        else if(settings is CategoryShopSettingsModel categoryShopSettings)    
                        {                           
                            shopImport.ShopSettingTabs.ShopCategoriesSettings = categoryShopSettings;
                            shopImport.ShopSettingTabs.ShopCategoriesSettings.Init(shopImport.ShopGuid);
                            return true;
                        }
                        return false;
                    }
                }

            case TabType.Products:
                {
                    if (settings is not ProductsImportSettingsModel productsImportSettings)
                        return false;
                    shopImport.ImportProducts = productsImportSettings;
                    return true;
                }

            case TabType.Categories:
                {
                    if (settings is not CategoriesImportSettingsModel categoriesImportSettings)
                        return false;
                    shopImport.ImportCategories = categoriesImportSettings;
                    return true;
                }

            default:                
                    return false;
                
        }
    }

    private static void Init(this ShopSettingsModel shopSettings, Guid shopGuid)
    {
        shopSettings.ShopGuid = shopGuid;
        shopSettings.Services.ForEach(s => { s.ShopGuid = shopGuid; s.ShopSettingsGuid = shopSettings.Guid; });
    }    

    public static ShopSettingsModel? GetShopSettingsByType(this ShopSettingTabsModel shopSettingTabs, ShopSettingType shopSettingType)
    {
        if (shopSettingType == ShopSettingType.Service)
            throw new InvalidOperationException("Service tab not available for shop settings.");

        return shopSettingType switch
        {
            ShopSettingType.Product => shopSettingTabs.ShopProductsSettings,
            ShopSettingType.Category => shopSettingTabs.ShopCategoriesSettings,
            _ => throw new InvalidOperationException($"No shop settings type with value {shopSettingType}"),
        };
    }

    public static ShopSettingsModel? GetShopSettingsByGuid(this ShopSettingTabsModel shopSettingTabs, Guid shopSettingsGuid)
    {
        return shopSettingTabs?.ShopProductsSettings?.Guid == shopSettingsGuid
            ? shopSettingTabs.ShopProductsSettings
            : (shopSettingTabs?.ShopCategoriesSettings?.Guid == shopSettingsGuid ? shopSettingTabs.ShopCategoriesSettings : null);
    }

    public static void UpdateServiceSettings(this ShopSettingsModel shopSettings, string serviceName, ServiceSettingsModel serviceSettings)
    {
        switch (serviceName)
        {
            case nameof(ShopSettingsModel.ImportService):
                {
                    shopSettings.ImportService.Update(serviceSettings);
                    shopSettings.AddOrUpdateServices(shopSettings.ImportService);
                    return;
                }

            case nameof(ShopSettingsModel.WebLoader):
                {
                    shopSettings.WebLoader.Update(serviceSettings);
                    shopSettings.AddOrUpdateServices(shopSettings.WebLoader);
                    return;
                }

            case nameof(ShopSettingsModel.BrowserDataLoader):
                {
                    shopSettings.BrowserDataLoader?.Update(serviceSettings);
                    if (shopSettings.BrowserDataLoader != null)
                        shopSettings.AddOrUpdateServices(shopSettings.BrowserDataLoader);
                    return;
                }

            case nameof(ShopSettingsModel.RequestHeaders):
                {
                    shopSettings.RequestHeaders?.Update(serviceSettings);
                    if (shopSettings.RequestHeaders != null)
                        shopSettings.AddOrUpdateServices(shopSettings.RequestHeaders);
                    return;
                }

            default:
                {
                    shopSettings.AddOrUpdateServices(serviceSettings);
                    return;
                }
        }
    }

    public static void UpdateServiceSettings(this ShopSettingsModel shopSettings, ServiceSettingsModel serviceSettings)
    {
        shopSettings.UpdateServiceSettings(serviceSettings.Name, serviceSettings);
    }

    public static void SetServiceSettings(this ShopSettingsModel shopSettings, ServiceSettingsModel serviceSettings)
    {
        switch (serviceSettings?.Name)
        {
            case nameof(ShopSettingsModel.ImportService):
                shopSettings.ImportService = shopSettings.CreateServiceSettingsModel(nameof(ShopSettingsModel.ImportService));
                shopSettings.ImportService.Update(serviceSettings);
                AddOrUpdateServices(shopSettings, shopSettings.ImportService);
                return;
            
            case nameof(ShopSettingsModel.BrowserDataLoader):
                shopSettings.BrowserDataLoader = shopSettings.CreateServiceSettingsModel(nameof(ShopSettingsModel.BrowserDataLoader));
                shopSettings.BrowserDataLoader.Update(serviceSettings);
                AddOrUpdateServices(shopSettings, shopSettings.BrowserDataLoader);
                return;
            
            case nameof(ShopSettingsModel.RequestHeaders):
                shopSettings.RequestHeaders = shopSettings.CreateServiceSettingsModel(nameof(ShopSettingsModel.RequestHeaders));
                shopSettings.RequestHeaders.Update(serviceSettings);
                AddOrUpdateServices(shopSettings, shopSettings.RequestHeaders);
                return;
            
            case nameof(ShopSettingsModel.WebLoader):
                shopSettings.WebLoader = shopSettings.CreateServiceSettingsModel(nameof(ShopSettingsModel.WebLoader));
                shopSettings.WebLoader.Update(serviceSettings);
                AddOrUpdateServices(shopSettings, shopSettings.WebLoader);
                return;

            default:
                AddOrUpdateServices(shopSettings, serviceSettings);
                return;

        }
    }    

    public static ServiceSettingsModel? GetServiceSettings(this ShopSettingsModel shopSettings, string serviceName)
    {
        return serviceName switch
        {
            nameof(ShopSettingsModel.ImportService) => shopSettings.ImportService,
            nameof(ShopSettingsModel.WebLoader) => shopSettings.WebLoader,
            nameof(ShopSettingsModel.BrowserDataLoader) => shopSettings.BrowserDataLoader,
            nameof(ShopSettingsModel.RequestHeaders) => shopSettings.RequestHeaders,
            _ => null,
        };
    }

    public static bool IsEqual(this ServiceSettingsModel source, ServiceSettingsModel target)
    {
        return target.Guid == source.Guid
        || (!string.IsNullOrEmpty(source.Name) && !string.IsNullOrEmpty(target.Name) && target.Name == source.Name)
        || target.ServiceTypeName == source.ServiceTypeName;
    }

    public static void AddOrUpdateServices(this ShopSettingsModel shopSettings, ServiceSettingsModel source)
    {
        var service = shopSettings.Services.FirstOrDefault(s => s.IsEqual(source));
        if (service != null)
            service.Update(source);
        else
        {
            source.ShopGuid = shopSettings.ShopGuid;
            source.ShopSettingsGuid = shopSettings.Guid;
            shopSettings.Services.Add(source);
        }
    }
   

    public static SettingsModelBase? DeserializeWithNumberHandling(this string json, Type type)
    {
        var option = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString, };
        return JsonSerializer.Deserialize(json, type, option) as SettingsModelBase;
    }

    public static ShopSettingsModel? GetShopSettingsFromJson(this string json, ShopSettingType shopSettingType)
    {
        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.IgnorePropertiesForSerialize(typeof(ServiceSettingsModel),
                nameof(ServiceSettingsModel.Value)) }
            }
        };

        return shopSettingType == ShopSettingType.Product
            ? JsonSerializer.Deserialize<ProductShopSettingsModel>(json, option)
            : JsonSerializer.Deserialize<CategoryShopSettingsModel>(json, option);
    }

    public static async Task<ShopSettingsModel?> GetShopSettingsFromJsonAsync(this Stream jsonStream, ShopSettingType shopSettingType)
    {
        var option = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.IgnorePropertiesForSerialize(typeof(ServiceSettingsModel),
                nameof(ServiceSettingsModel.Value)) }
            }
        };

        return shopSettingType == ShopSettingType.Product
            ? await JsonSerializer.DeserializeAsync<ProductShopSettingsModel>(jsonStream, option)
            : await JsonSerializer.DeserializeAsync<CategoryShopSettingsModel>(jsonStream, option);
    }
}
