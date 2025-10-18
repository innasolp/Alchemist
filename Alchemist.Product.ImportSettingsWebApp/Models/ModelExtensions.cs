using Alchemist.Import.Settings.Extensions;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

internal static class ModelExtensions
{
    internal static ServiceSettingsModel GetService(this ShopImportSettingsModel shopImportSettings, Guid guid)
    {
        return shopImportSettings.Services.FirstOrDefault(s=>s.Value.Guid == guid).Value;
    }

    internal static void Updateservice(this ServiceSettingsModel target, ServiceSettingsModel source)
    {
        if (!(target.Name.IsPrimaryServiceName() || source.Name.IsPrimaryServiceName()))
            target.Name = source.Name;

        target.Update(source); 
    }

    private static bool EqualsWithEmpty(this string? target, string? source)
    {
        return (string.IsNullOrEmpty(target) && string.IsNullOrEmpty(source))
            || target?.Equals(source, StringComparison.OrdinalIgnoreCase) == true;
}

    internal static bool ServiceEquals(this ServiceSettingsModel target, ServiceSettingsModel source)
    {
        return (target.Name.IsPrimaryServiceName() || target.Name.Equals(source.Name))
            && target.ServiceTypeName.EqualsWithEmpty(source.ServiceTypeName)
            && target.ServiceProviderPath.EqualsWithEmpty(source.ServiceProviderPath)
            && target.AssemblyPath.EqualsWithEmpty(source.AssemblyPath)
            && target.ImplementationTypeName.EqualsWithEmpty(source.ImplementationTypeName);
    }

    private static bool ShopSettingsIsEquals(this ShopImportSettingsModel target, ShopImportSettingsModel source)
    {
        return target.Name.EqualsWithEmpty(source.Name)
            && target.Perfomance == source.Perfomance;
    }

    internal static bool ServicesAreEquals(this IDictionary<string, ServiceSettingsModel> target,  IDictionary<string, ServiceSettingsModel> source)
    {
        return target.Count == source.Count && 
            target.All(ts=>source.Any(s=>s.Key == ts.Key && ts.Value.ServiceEquals(s.Value)));
    }

    private static bool IsEquals(this CategoryUrlModel target, CategoryUrlModel source)
    {
        return target.Item == source.Item && target.Url.EqualsWithEmpty(source.Url);
    }

    internal static bool ProductShopSettingsFieldsEquals(this ProductShopImportSettingsModel target, ProductShopImportSettingsModel source)
    {
        return target.ShopSettingsIsEquals(source)
         && target.ProductUrlFormat.EqualsWithEmpty(source.ProductUrlFormat)
            && target.CategoryUrlFormat.EqualsWithEmpty(source.CategoryUrlFormat)
            && target.PageProductCount.Equals(source.PageProductCount);
    }

    internal static bool CategoryShopSettingsFieldsEquals(this CategoryShopImportSettingsModel target, CategoryShopImportSettingsModel source)
    {
        return target.ShopSettingsIsEquals(source) &&  target.CategorySourceUrl.EqualsWithEmpty(source.CategorySourceUrl);
    }

    internal static bool ProductShopSettingsIsEquals(this ProductShopImportSettingsModel target, ProductShopImportSettingsModel source)
    {
        if (!target.ShopSettingsIsEquals(source)) return false;

        if( !( target.ProductUrlFormat.EqualsWithEmpty(source.ProductUrlFormat)
            && target.CategoryUrlFormat.EqualsWithEmpty(source.CategoryUrlFormat)
            && target.PageProductCount.Equals(source.PageProductCount))) return false;

        if(!target.Services.ServicesAreEquals(source.Services)) return false;

        return target.RootCategories.Count == source.RootCategories.Count &&
            target.RootCategories.All(ts => source.RootCategories.Any(s => s.IsEquals(ts)));
    }

    internal static bool CategoryShopSettingsIsEquals(this CategoryShopImportSettingsModel target, CategoryShopImportSettingsModel source)
    {
        if (!target.ShopSettingsIsEquals(source)) return false;

        if(!target.CategorySourceUrl.EqualsWithEmpty(source.CategorySourceUrl)) return false;

        return target.Services.ServicesAreEquals(source.Services);
    }

    internal static void UpdateServices(this ShopImportSettingsModel shopImportSettings, IDictionary<string, ServiceSettingsModel> services)
    {
        var targetServices = shopImportSettings.Services.ToDictionary();

        foreach (var targetService in targetServices)
        {
            var sourceService = services.FirstOrDefault(s=>s.Key == targetService.Key);
            if (sourceService.Value != null)
                targetService.Value.Update(sourceService.Value);
            else
                shopImportSettings.Services.Remove(targetService.Key);
        }

        var newServices = services.Where(s=>!targetServices.ContainsKey(s.Key));
        foreach (var newService in newServices)
            shopImportSettings.Services.Add(newService.Key, newService.Value);
    }

    internal static void UpdateCategorySources(this ProductShopImportSettingsModel productShopImportSettings, List<CategoryUrlModel> categoryUrls)
    {
        var targetCategoryUrls = productShopImportSettings.RootCategories.ToList();
        foreach(var targetCategoryUrl in targetCategoryUrls)
        {
            var sourceCategoryUrl = categoryUrls.FirstOrDefault(c => c.Item == targetCategoryUrl.Item);
            if (sourceCategoryUrl == null)
                productShopImportSettings.RootCategories.Remove(targetCategoryUrl);
            else
                targetCategoryUrl.Url = sourceCategoryUrl.Url;
        }

        var newCategoryUrls = categoryUrls.Where(c=>!targetCategoryUrls.Any(tc=>tc.Item == c.Item));
        foreach (var newCategoryUrl in newCategoryUrls)
            productShopImportSettings.RootCategories.Add(newCategoryUrl);
    }

    internal static void UpdateFields(this ShopImportSettingsModel target, ShopImportSettingsModel source)
    {
        target.Name = source.Name;
        target.Perfomance = source.Perfomance;
    }

    internal static void UpdateProductShopImportSettings(this ProductShopImportSettingsModel target, ProductShopImportSettingsModel source)
    { 
        target.ProductUrlFormat = source.ProductUrlFormat;
        target.CategoryUrlFormat = source.CategoryUrlFormat;
        target.PageProductCount = source.PageProductCount;

        target.UpdateCategorySources(source.RootCategories);
    }

    internal static void UpdateCategoryShopImportSettings(this CategoryShopImportSettingsModel target, CategoryShopImportSettingsModel source)
    { 
        target.CategorySourceUrl = source.CategorySourceUrl;
    }

    internal static bool IsEmpty(this ServiceSettingsModel serviceSettings)
    {
        return string.IsNullOrEmpty(serviceSettings.ServiceTypeName)
               && string.IsNullOrEmpty(serviceSettings.AssemblyPath)
               && string.IsNullOrEmpty(serviceSettings.ServiceProviderPath)
               && string.IsNullOrEmpty(serviceSettings.ImplementationTypeName);
    }

    internal static bool ShopImportSettingsIsEmpty(this ShopImportSettingsModel shopImportSettings)
    {
        return string.IsNullOrEmpty(shopImportSettings.Name)
              && !shopImportSettings.Services.Any();
    }

    internal static bool IsEmpty(this ProductShopImportSettingsModel productSettings)
    {
        return productSettings.ShopImportSettingsIsEmpty()
               && string.IsNullOrEmpty(productSettings.ProductUrlFormat)
               && string.IsNullOrEmpty(productSettings.CategoryUrlFormat)
               && productSettings.PageProductCount == null
               && productSettings.RootCategories?.Any() == false;
    }

    internal static bool IsEmpty(this CategoryShopImportSettingsModel categorySettings)
    {
        return categorySettings.ShopImportSettingsIsEmpty()
               && string.IsNullOrEmpty(categorySettings.CategorySourceUrl);
    }
}
