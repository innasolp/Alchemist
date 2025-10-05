using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Interfaces;
using SettingsCommon = Alchemist.Import.Settings.Extensions.Common;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public static class ModelAssert
{
    internal static void EqualFields(IShopImportSettingsModel expected, IShopImportSettingsModel result)
    {
        Assert.Equal(expected.Type, result.Type);
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.Perfomance, result.Perfomance);

        if (expected.Type == ShopSettingType.Product)
            EqualProductShopSettingsFields(expected as IProductShopSettingsModel, result as IProductShopSettingsModel);
        else if (expected.Type == ShopSettingType.Category)
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


    internal static void NotEqualFields(IShopImportSettingsModel expected, IShopImportSettingsModel result)
    {
        Assert.NotEqual(expected.Name, result.Name);        

        if (expected.Type == ShopSettingType.Product)
            NotEqualProductShopSettingsFields(expected as IProductShopSettingsModel, result as IProductShopSettingsModel);
        else if (expected.Type == ShopSettingType.Category)
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

    internal static void EqualServices(IShopImportSettingsModel expected, IShopImportSettingsModel result)
    {
        foreach(var primaryServiceName in SettingsCommon.GetPrimaryServiceNames())
        {
            var resultService = result.GetService(primaryServiceName) as IServiceSettingsModel;
            var expectedService = expected.GetService(primaryServiceName) as IServiceSettingsModel;

            Assert.True((resultService != null && expectedService != null) || (resultService == null && expectedService == null));
            
            if (resultService != null && expectedService != null)
                EqualFields(expectedService, resultService);
        }
    }

    internal static void NotEqualServices(IShopImportSettingsModel expected, IShopImportSettingsModel result)
    {
        foreach (var primaryServiceName in SettingsCommon.GetPrimaryServiceNames())
        {
            var expectedService = expected.GetService(primaryServiceName) as IServiceSettingsModel;
            if (expectedService == null)
                continue;

            var resultService = result.GetService(primaryServiceName) as IServiceSettingsModel;
            
            NotEqualFields(expectedService, resultService);
        }
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
