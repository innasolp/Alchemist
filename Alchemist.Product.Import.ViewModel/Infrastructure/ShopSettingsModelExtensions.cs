using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ShopSettingsModelExtensions
{
    public static bool IsEmpty(this IShopServicesSettingsModel shopServicesSettingsModel)
    {
        return string.IsNullOrEmpty(shopServicesSettingsModel.Name)
            && shopServicesSettingsModel.BrowserDataLoader.IsEmpty()
            && shopServicesSettingsModel.WebLoader.IsEmpty()
            && shopServicesSettingsModel.ImportService.IsEmpty()
            && shopServicesSettingsModel.RequestHeaders.IsEmpty();
    }
    public static void UpdateServiceSettings(this IShopServicesSettingsModel shopSettings, string serviceName, IServiceSettingsModel serviceSettings)
    {
        switch (serviceName)
        {
            case nameof(IShopServicesSettingsModel.ImportService):
                {
                    shopSettings.ImportService.Update(serviceSettings);
                    shopSettings.AddOrUpdateService(shopSettings.ImportService);
                    return;
                }

            case nameof(IShopServicesSettingsModel.WebLoader):
                {
                    shopSettings.WebLoader.Update(serviceSettings);
                    shopSettings.AddOrUpdateService(shopSettings.WebLoader);
                    return;
                }

            case nameof(IShopServicesSettingsModel.BrowserDataLoader):
                {
                    shopSettings.BrowserDataLoader.Update(serviceSettings);
                    shopSettings.AddOrUpdateService(shopSettings.BrowserDataLoader);
                    return;
                }

            case nameof(IShopServicesSettingsModel.RequestHeaders):
                {
                    shopSettings.RequestHeaders.Update(serviceSettings);
                    shopSettings.AddOrUpdateService(shopSettings.RequestHeaders);
                    return;
                }

            default:
                {
                    shopSettings.AddOrUpdateService(serviceSettings);
                    return;
                }
        }
    }

    private static void AddOrUpdateService(this IShopServicesSettingsModel shopSettings, IServiceSettingsModel service)
    {
        var existingService = shopSettings.Services.OfType<IServiceSettingsModel>().FirstOrDefault(s => s.Guid == service.Guid);
        if (existingService != null)
            existingService.Update(service);
        else
            shopSettings.Services.Add(service);
    }

    private static void UpdateShopSettingsCore(this IShopServicesSettingsModel target, IShopServicesSettingsModel source)
    {
        target.Name = source.Name;
        target.FileName = source.FileName;
        target.Perfomance = source.Perfomance;

        target.ImportService.Update(source.ImportService);
        target.BrowserDataLoader.Update(source.BrowserDataLoader);
        target.RequestHeaders.Update(source.RequestHeaders);
        target.WebLoader.Update(source.WebLoader);

        var sourceServices = source.Services.OfType<IServiceSettingsModel>().ToList();

        var servicesForRemove = new List<IServiceSettingsModel>(target.Services.OfType<IServiceSettingsModel>().Where(r =>
                    !sourceServices.Any(s => s.Guid == r.Guid) || sourceServices.First(s => s.Guid == r.Guid).IsEmpty()));

        foreach (var service in servicesForRemove)
            target.Services.Remove(service);

        foreach (var service in sourceServices)
        {
            var targetService = target.Services.OfType<IServiceSettingsModel>().FirstOrDefault(s => s.Guid == service.Guid);
            if (targetService == null)
            {
                service.ShopSettingsGuid = target.Guid;
                service.ShopGuid = target.ShopGuid;
                target.Services.Add(service);
            }
            else
                targetService.Update(service);
        }
    }

    private static void UpdateShopSettingsWithoutServices(this IShopServicesSettingsModel target, IShopServicesSettingsModel source)
    {
        target.Name = source.Name;
        target.FileName = source.FileName;
        target.Perfomance = source.Perfomance;
    }

    public static void UpdateProductShopSettings(this IProductShopSettingsModel target, IProductShopSettingsModel source)
    {
        target.UpdateShopSettingsCore(source);

        target.ProductUrlFormat = source.ProductUrlFormat;
        target.CategoryUrlFormat = source.CategoryUrlFormat;
        target.PageProductCount = source.PageProductCount;

        var sourceCategories = source.RootCategories.OfType<ICategoryUrlModel>().ToList();
        var rootCategoriesForRemove = new List<ICategoryUrlModel>(target.RootCategories.OfType<ICategoryUrlModel>().Where(r => !sourceCategories.Any(s => s.Guid == r.Guid)));
        foreach (var category in rootCategoriesForRemove)
            target.RootCategories.Remove(category);

        foreach (var rootCategory in sourceCategories)
        {
            var targetRootCategory = target.RootCategories.OfType<ICategoryUrlModel>().FirstOrDefault(r => r.Guid == rootCategory.Guid);
            if (targetRootCategory == null)
            {
                rootCategory.ShopSettingsGuid = target.Guid;
                target.RootCategories.Add(rootCategory);
            }
            else
                targetRootCategory.Update(rootCategory);
        }
    }

    public static void UpdateProductShopSettingsWithoutServices(this IProductShopSettingsModel target, IProductShopSettingsModel source)
    {
        target.UpdateShopSettingsWithoutServices(source);
        target.ProductUrlFormat = source.ProductUrlFormat;
        target.CategoryUrlFormat = source.CategoryUrlFormat;
        target.PageProductCount = source.PageProductCount;
    }
   
    public static void Update(this ICategoryUrlModel target, ICategoryUrlModel source)
    {
        target.Item = source.Item;
        target.Url = source.Url;
    }

    public static void UpdateCategoryShopSettings(this ICategoryShopSettingsModel target, ICategoryShopSettingsModel source)
    {
        target.UpdateShopSettingsCore(source);

        target.CategorySourceUrl = source.CategorySourceUrl;
    }

    public static void UpdateCategoryShopSettingsWithoutServices(this ICategoryShopSettingsModel target, ICategoryShopSettingsModel source)
    {
        target.UpdateShopSettingsWithoutServices(source);
        target.CategorySourceUrl = source.CategorySourceUrl;
    }

    public static void Update(this IShopServicesSettingsModel target, IShopImportSettings source)
    {
        if (target.ShopSettingType != source.ShopSettingType)
            throw new InvalidOperationException("different shop settings types");

        if (target.ShopSettingType == ShopSettingType.Service)
            throw new InvalidOperationException("invalid shop setting type");

        if (source.ShopSettingType == ShopSettingType.Product && source is IProductShopSettingsModel sourceProducts
            && target is IProductShopSettingsModel targetProducts)
            targetProducts.UpdateProductShopSettings(sourceProducts);
        else if (source.ShopSettingType == ShopSettingType.Category && source is ICategoryShopSettingsModel sourceCategories
            && target is ICategoryShopSettingsModel targetCategories)
            targetCategories.UpdateCategoryShopSettings(sourceCategories);
        else
            throw new InvalidOperationException("different source and target types");
    }

    public static IShopServicesSettingsModel? GetShopSettingsByType(this IShopSettingTabsModel shopSettingTabs, ShopSettingType shopSettingType)
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

    internal static IShopServicesSettingsModel? GetShopSettingsByGuid(this IShopSettingTabsModel shopSettingTabs, Guid shopSettingsGuid)
    {
        return shopSettingTabs?.ShopProductsSettings?.Guid == shopSettingsGuid
            ? shopSettingTabs.ShopProductsSettings
            : (shopSettingTabs?.ShopCategoriesSettings?.Guid == shopSettingsGuid ? shopSettingTabs.ShopCategoriesSettings : null);
    }

    public static bool FieldsEquals(this IShopServicesSettingsModel source, IShopServicesSettingsModel? other)
    {
        return other != null && source.ShopSettingType == other.ShopSettingType
            && ((string.IsNullOrEmpty(source.Name) && string.IsNullOrEmpty(other.Name))
                || string.Equals(source.Name, other.Name, StringComparison.InvariantCultureIgnoreCase))
            && source.ImportService.Equals(other.ImportService) != false
            && source.WebLoader.Equals(other.WebLoader) != false
            && source.BrowserDataLoader.Equals(other.BrowserDataLoader) != false
            && source.RequestHeaders.Equals(other.RequestHeaders) != false;
    }

    public static bool FieldsEquals(this IShopServicesSettingsModel source, IShopImportSettings? other, bool withServices = true)
    {
        return other != null && source.ShopSettingType == other.ShopSettingType
            && ((string.IsNullOrEmpty(source.Name) && string.IsNullOrEmpty(other.Name))
                || string.Equals(source.Name, other.Name, StringComparison.InvariantCultureIgnoreCase))
                && (!withServices || 
            ( source.ImportService.FieldsEquals(other.ImportService) != false
            && source.WebLoader.FieldsEquals(other.WebLoader) != false
            && source.BrowserDataLoader.FieldsEquals(other.BrowserDataLoader) != false
            && source.RequestHeaders.FieldsEquals(other.RequestHeaders) != false))
            && ((source.ShopSettingType == ShopSettingType.Product && EqualsProductShopSettings(source as IProductShopSettingsModel, other as IProductShopImportSettings))
            || (source.ShopSettingType == ShopSettingType.Category && EqualsCategoryShopSettings(source as ICategoryShopSettingsModel, other as ICategoryShopImportSettings)));
    }

    private static bool EqualsProductShopSettings(this IProductShopSettingsModel source, IProductShopImportSettings? other)
    {
        return other != null
            && ((string.IsNullOrEmpty(source.ProductUrlFormat) && string.IsNullOrEmpty(other.ProductUrlFormat))
                || string.Equals(source.ProductUrlFormat, other.ProductUrlFormat, StringComparison.InvariantCultureIgnoreCase))
            && ((string.IsNullOrEmpty(source.CategoryUrlFormat) && string.IsNullOrEmpty(other.CategoryUrlFormat))
                || string.Equals(source.CategoryUrlFormat, other.CategoryUrlFormat, StringComparison.InvariantCultureIgnoreCase))
                && (source.RootCategories.Count == other.RootCategories.Length
                      && source.RootCategories.OfType<ICategoryUrlModel>().All(c => other.RootCategories.Any(r => r.Item == c.Item && r.Url == c.Url)));
    }

    private static bool EqualsCategoryShopSettings(this ICategoryShopSettingsModel source, ICategoryShopImportSettings? other)
    {
        return other != null
            && ((string.IsNullOrEmpty(source.CategorySourceUrl) && string.IsNullOrEmpty(other.CategorySourceUrl))
                || string.Equals(source.CategorySourceUrl, other.CategorySourceUrl, StringComparison.InvariantCultureIgnoreCase));
    }    
}
