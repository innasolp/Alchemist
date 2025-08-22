using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ShopSettingsModelExtensions
{
    public static bool IsEmpty(this IShopImportSettingsModel shopServicesSettingsModel)
    {
        return string.IsNullOrEmpty(shopServicesSettingsModel.Name) &&
            shopServicesSettingsModel.GetPrimaryServices<IServiceSettingsModel>().All(s => s.IsEmpty());
    }    

    private static void UpdateShopSettingsCore(this IModelFactory modelFactory, IShopImportSettingsModel target, IShopImportSettingsModel source)
    {
        target.Name = source.Name;
        target.FileName = source.FileName;
        target.Perfomance = source.Perfomance;

        var sourceServices = source.Services.OfType<IServiceSettingsModel>().ToList();

        var targetServicesForRemove = new List<IServiceSettingsModel>(target.Services.OfType<IServiceSettingsModel>().Where(r =>
                    !sourceServices.Any(s => s.Guid == r.Guid) || sourceServices.First(s => s.Guid == r.Guid).IsEmpty()));

        foreach (var service in targetServicesForRemove)
            target.Services.Remove(service);

        foreach (var sourceService in sourceServices)
        {
            //todo name or guid ???
            var targetService = target.GetService(sourceService.Name ?? sourceService.ServiceTypeName);
            if (targetService == null)
            {                
                targetService = modelFactory.CreateServiceSettingsModel( shopId: target.ShopId, 
                    id: sourceService.Id,
                    parentId: target.Id,
                    shopGuid: target.ShopGuid, 
                    shopSettingsGuid: target.Guid);
                targetService.Name = sourceService.Name;
                targetService.Update(sourceService);
                target.Services.Add(targetService);
            }
            else
                targetService.Update(sourceService);
        }
    }

    private static void UpdateShopSettingsWithoutServices(this IShopImportSettingsModel target, IShopImportSettingsModel source)
    {
        target.Name = source.Name;
        target.FileName = source.FileName;
        target.Perfomance = source.Perfomance;
    }

    public static void UpdateProductShopSettings(this IModelFactory modelFactory, IProductShopSettingsModel target, IProductShopSettingsModel source)
    {
        modelFactory.UpdateShopSettingsCore(target, source);

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
                targetRootCategory = modelFactory.CreateRootCategory(target.Guid);
                targetRootCategory.Update(rootCategory);
                target.RootCategories.Add(targetRootCategory);
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

    public static void UpdateCategoryShopSettings(this IModelFactory modelFactory, ICategoryShopSettingsModel target, ICategoryShopSettingsModel source)
    {
        modelFactory.UpdateShopSettingsCore(target,source);

        target.CategorySourceUrl = source.CategorySourceUrl;
    }

    public static void UpdateCategoryShopSettingsWithoutServices(this ICategoryShopSettingsModel target, ICategoryShopSettingsModel source)
    {
        target.UpdateShopSettingsWithoutServices(source);
        target.CategorySourceUrl = source.CategorySourceUrl;
    }

    public static void Update(this IModelFactory modelFactory, IShopImportSettingsModel target, IShopImportSettings source)
    {
        if (target.ShopSettingType != source.ShopSettingType)
            throw new InvalidOperationException("different shop settings types");

        if (target.ShopSettingType == ShopSettingType.Service)
            throw new InvalidOperationException("invalid shop setting type");

        if (source.ShopSettingType == ShopSettingType.Product && source is IProductShopSettingsModel sourceProducts
            && target is IProductShopSettingsModel targetProducts)
            modelFactory.UpdateProductShopSettings(targetProducts, sourceProducts);
        else if (source.ShopSettingType == ShopSettingType.Category && source is ICategoryShopSettingsModel sourceCategories
            && target is ICategoryShopSettingsModel targetCategories)
            modelFactory.UpdateCategoryShopSettings(targetCategories, sourceCategories);
        else
            throw new InvalidOperationException("different source and target types");
    }

    public static IShopImportSettingsModel? GetShopSettingsByType(this IShopSettingTabsModel shopSettingTabs, ShopSettingType shopSettingType)
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

    internal static IShopImportSettingsModel? GetShopSettingsByGuid(this IShopSettingTabsModel shopSettingTabs, Guid shopSettingsGuid)
    {
        return shopSettingTabs?.ShopProductsSettings?.Guid == shopSettingsGuid
            ? shopSettingTabs.ShopProductsSettings
            : (shopSettingTabs?.ShopCategoriesSettings?.Guid == shopSettingsGuid ? shopSettingTabs.ShopCategoriesSettings : null);
    }

 
 
}
