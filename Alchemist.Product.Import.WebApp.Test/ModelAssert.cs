using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public static class ModelAssert
{
    internal static void EqualFields(ShopSettingsModel expected, ShopSettingsModel result)
    {
        Assert.Equal(expected.Name, result.Name);
        Assert.Equal(expected.Caption, result.Caption);
        Assert.Equal(expected.Url, result.Url);
        Assert.Equal(expected.Perfomance, result.Perfomance);
    }

    internal static void NotEqualFields(ShopSettingsModel expected, ShopSettingsModel result)
    {
        Assert.NotEqual(expected.Name, result.Name);
        Assert.NotEqual(expected.Caption, result.Caption);
        Assert.NotEqual(expected.Url, result.Url);
        Assert.NotEqual(expected.Perfomance, result.Perfomance);
    }

    internal static void EqualServices(ShopSettingsModel expected, ShopSettingsModel result)
    {
        Assert.True((result.ImportService != null && expected.ImportService != null) ||
            (result.ImportService == null && expected.ImportService == null));
        EqualFields(expected.ImportService, result.ImportService);

        Assert.True((result.BrowserDataLoader != null && expected.BrowserDataLoader != null) ||
            (result.BrowserDataLoader == null && expected.BrowserDataLoader == null));
        EqualFields(expected.BrowserDataLoader, result.BrowserDataLoader);

        Assert.True((result.RequestHeaders != null && expected.RequestHeaders != null) ||
            (result.RequestHeaders == null && expected.RequestHeaders == null));
        EqualFields(expected.RequestHeaders, result.RequestHeaders);

        Assert.True((result.WebLoader != null && expected.WebLoader != null) ||
            (result.WebLoader == null && expected.WebLoader == null));
        EqualFields(expected.WebLoader, result.WebLoader);
    }

    internal static void NotEqualServices(ShopSettingsModel expected, ShopSettingsModel result)
    {
        if (expected.ImportService != null)
            NotEqualFields(expected.ImportService, result.ImportService);

        if (expected.BrowserDataLoader != null)
            NotEqualFields(expected.BrowserDataLoader, result.BrowserDataLoader);

        if (expected.RequestHeaders != null)
            NotEqualFields(expected.RequestHeaders, result.RequestHeaders);

        if (expected.WebLoader != null)
            NotEqualFields(expected.WebLoader, result.WebLoader);
    }

    internal static void EqualFields(ServiceSettingsModel expected, ServiceSettingsModel result)
    {
        Assert.Equal(expected.ServiceTypeName, result.ServiceTypeName);
        Assert.Equal(expected.ServiceProviderPath, result.ServiceProviderPath);
        Assert.Equal(expected.AssemblyPath, result.AssemblyPath);
        Assert.Equal(expected.ImplementationTypeName, result.ImplementationTypeName);
        Assert.Equal(expected.Value?.ToString(), result.Value?.ToString());
    }

    internal static void NotEqualFields(ServiceSettingsModel expected, ServiceSettingsModel result)
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
