using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public static class ModelAssert
{
    internal static void EqualFields(IShopServicesSettingsModel expected, IShopServicesSettingsModel result)
    {
        Assert.Equal(expected.ShopSettingType, result.ShopSettingType);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.Perfomance, result.Perfomance);

        if (expected.ShopSettingType == Alchemist.Import.Settings.Interfaces.ShopSettingType.Product)
            EqualProductShopSettingsFields(expected as IProductShopSettingsModel, result as IProductShopSettingsModel);
        else if (expected.ShopSettingType == Alchemist.Import.Settings.Interfaces.ShopSettingType.Category)
            EqualCategoryShopSettingsFields(expected as ICategoryShopSettingsModel, result as ICategoryShopSettingsModel);
    }

    private static void EqualProductShopSettingsFields(IProductShopSettingsModel expected, IProductShopSettingsModel result)
    {
        Assert.Equal(expected.ProductUrlFormat, result.ProductUrlFormat);
        Assert.Equal(expected.CategoryUrlFormat, result.CategoryUrlFormat);
        Assert.Equal(expected.PageProductCount, result.PageProductCount);

        Assert.Equal(expected.RootCategories.Count, result.RootCategories.Count);   
        Assert.True(expected.RootCategories.OfType<ICategoryUrlModel>().All(c=>result.RootCategories.OfType<ICategoryUrlModel>().Any(r=>r.Item == c.Item && r.Url == c.Url)));
    }

    private static void EqualCategoryShopSettingsFields(ICategoryShopSettingsModel expected, ICategoryShopSettingsModel result)
    {
        Assert.Equal(expected.CategorySourceUrl, result.CategorySourceUrl);
    }


    internal static void NotEqualFields(IShopServicesSettingsModel expected, IShopServicesSettingsModel result)
    {
        Assert.NotEqual(expected.Name, result.Name);        

        if (expected.ShopSettingType == Alchemist.Import.Settings.Interfaces.ShopSettingType.Product)
            NotEqualProductShopSettingsFields(expected as IProductShopSettingsModel, result as IProductShopSettingsModel);
        else if (expected.ShopSettingType == Alchemist.Import.Settings.Interfaces.ShopSettingType.Category)
            NotEqualCategoryShopSettingsFields(expected as ICategoryShopSettingsModel, result as ICategoryShopSettingsModel);
    }

    private static void NotEqualProductShopSettingsFields(IProductShopSettingsModel expected, IProductShopSettingsModel result)
    {
        Assert.NotEqual(expected.ProductUrlFormat, result.ProductUrlFormat);
        Assert.NotEqual(expected.CategoryUrlFormat, result.CategoryUrlFormat);
        Assert.NotEqual(expected.PageProductCount, result.PageProductCount);

        Assert.False(expected.RootCategories.Count == result.RootCategories.Count &&
        expected.RootCategories.OfType<ICategoryUrlModel>().All(c => result.RootCategories.OfType<ICategoryUrlModel>().Any(r => r.Item == c.Item && r.Url == c.Url)));
    }

    private static void NotEqualCategoryShopSettingsFields(ICategoryShopSettingsModel expected, ICategoryShopSettingsModel result)
    {
        Assert.NotEqual(expected.CategorySourceUrl, result.CategorySourceUrl);
    }

    internal static void EqualServices(IShopServicesSettingsModel expected, IShopServicesSettingsModel result)
    {
        Assert.True((result.ImportService != null && expected.ImportService != null) ||
            (result.ImportService == null && expected.ImportService == null));
        EqualFields(expected.ImportService, result.ImportService);

        Assert.True((result.BrowserDataLoader != null && expected.BrowserDataLoader != null) ||
            (result.BrowserDataLoader == null && expected.BrowserDataLoader == null));
        EqualFields(expected.BrowserDataLoader, result.BrowserDataLoader);

        Assert.True((result.BrowserLauncher != null && expected.BrowserLauncher != null) ||
            (result.BrowserLauncher == null && expected.BrowserLauncher == null));
        EqualFields(expected.BrowserLauncher, result.BrowserLauncher);

        Assert.True((result.RequestHeaders != null && expected.RequestHeaders != null) ||
            (result.RequestHeaders == null && expected.RequestHeaders == null));
        EqualFields(expected.RequestHeaders, result.RequestHeaders);

        Assert.True((result.WebLoader != null && expected.WebLoader != null) ||
            (result.WebLoader == null && expected.WebLoader == null));
        EqualFields(expected.WebLoader, result.WebLoader);
    }

    internal static void NotEqualServices(IShopServicesSettingsModel expected, IShopServicesSettingsModel result)
    {
        if (expected.ImportService != null)
            NotEqualFields(expected.ImportService, result.ImportService);

        if (expected.BrowserDataLoader != null)
            NotEqualFields(expected.BrowserDataLoader, result.BrowserDataLoader);

        if (expected.BrowserLauncher != null)
            NotEqualFields(expected.BrowserLauncher, result.BrowserLauncher);

        if (expected.RequestHeaders != null)
            NotEqualFields(expected.RequestHeaders, result.RequestHeaders);

        if (expected.WebLoader != null)
            NotEqualFields(expected.WebLoader, result.WebLoader);
    }

    internal static void EqualFields(IServiceSettingsModel expected, IServiceSettingsModel result)
    {
        Assert.Equal(expected.ServiceTypeName, result.ServiceTypeName);
        Assert.Equal(expected.ServiceProviderPath, result.ServiceProviderPath);
        Assert.Equal(expected.AssemblyPath, result.AssemblyPath);
        Assert.Equal(expected.ImplementationTypeName, result.ImplementationTypeName);
        Assert.Equal(expected.JsonValue?.ToString(), result.JsonValue?.ToString());
    }

    internal static void NotEqualFields(IServiceSettingsModel expected, IServiceSettingsModel result)
    {
        if (!string.IsNullOrEmpty(expected.ServiceTypeName) && !string.IsNullOrEmpty(result.ServiceTypeName))
            Assert.NotEqual(expected.ServiceTypeName, result.ServiceTypeName);

        if (!string.IsNullOrEmpty(expected.ServiceProviderPath) && !string.IsNullOrEmpty(result.ServiceProviderPath))
            Assert.NotEqual(expected.ServiceProviderPath, result.ServiceProviderPath);

        if (!string.IsNullOrEmpty(expected.AssemblyPath) && !string.IsNullOrEmpty(result.AssemblyPath))
            Assert.NotEqual(expected.AssemblyPath, result.AssemblyPath);

        if (!string.IsNullOrEmpty(expected.ImplementationTypeName) && !string.IsNullOrEmpty(result.ImplementationTypeName))
            Assert.NotEqual(expected.ImplementationTypeName, result.ImplementationTypeName);
    }

    private static bool IsEqual(this IServiceSettingsModel source, IServiceSettingsModel target)
    {
        return target.Guid == source.Guid
        || (!string.IsNullOrEmpty(source.Name) && !string.IsNullOrEmpty(target.Name) && target.Name == source.Name)
        || (!string.IsNullOrEmpty(source.ServiceTypeName) && !string.IsNullOrEmpty(target.ServiceTypeName) &&
                    target.ServiceTypeName == source.ServiceTypeName);
    }

    internal static void EqualCollections(IEnumerable<ServiceSettingsModel> expected, IEnumerable<ServiceSettingsModel> result)
    {
        var expectedList = expected.ToList();
        var resultList = result.ToList();

        Assert.Equal(expectedList.Count, resultList.Count);
        expectedList.ForEach(serviceExpected =>
        {
            var serviceResult = resultList.FirstOrDefault(s => s.IsEqual(serviceExpected));

            Assert.NotNull(serviceResult);
            EqualFields(serviceExpected, serviceResult);
        });
    }
}
