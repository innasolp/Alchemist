using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Extensions;

public static class ServiceExtensions
{   
    public static bool IsPrimary(this IImportServiceSettings service)
    {
        var primaryServiceNames = Common.GetPrimaryServiceNames();
        return primaryServiceNames.Any(n => service.Name == n || service.ServiceTypeName == n);
    }

    public static bool IsPrimaryServiceName(this string serviceName)
    {
        var primaryServiceNames = Common.GetPrimaryServiceNames();
        return primaryServiceNames.Any(n => serviceName == n);
    }

    public static IImportServiceSettings? GetService(this IShopImportSettings shopImportSettings, string name)
    {
        return shopImportSettings.Services.OfType<IImportServiceSettings>().FirstOrDefault(s => s.Name == name || s.ServiceTypeName == name);
    }

    public static IImportServiceSettings? GetPrimaryService(this IShopImportSettings shopImportSettings, string name)
    {
        if (Common.GetPrimaryServiceNames().Contains(name))
            return shopImportSettings.GetService(name);

        throw new InvalidOperationException($"Service name {name} is not primary.");
    }

    public static IEnumerable<TService> GetPrimaryServices<TService>(this IShopImportSettings shopImportSettings)
        where TService : class, IImportServiceSettings
    {
        return shopImportSettings.Services.OfType<TService>().Where(s => s.IsPrimary());
    }

    public static IImportServiceSettings? GetImportService(this IShopImportSettings shopImportSettings)
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.ImportService));
    }   

    public static IImportServiceSettings? GetBrowserDataLoader(this IShopImportSettings shopImportSettings)
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.BrowserDataLoader));
    }

    public static IImportServiceSettings? GetBrowserLauncher(this IShopImportSettings shopImportSettings)
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.BrowserLauncher));
    }

    public static IImportServiceSettings? GetRequestHeaders(this IShopImportSettings shopImportSettings)
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.RequestHeaders));
    }

    public static IImportServiceSettings? GetWebLoader(this IShopImportSettings shopImportSettings)
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.WebLoader));
    }

}
