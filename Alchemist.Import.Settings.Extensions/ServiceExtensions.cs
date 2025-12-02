using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Extensions;

public static class ServiceExtensions
{   
    public static bool IsPrimaryServiceName(this string serviceName)
    {
        var primaryServiceNames = Common.GetPrimaryServiceNames();
        return primaryServiceNames.Any(n => serviceName == n);
    }

    public static TService? GetService<TService>(this IShopImportSettings shopImportSettings, string name)
        where TService : class, IServiceSettings
    {
        var key = shopImportSettings.Services.Keys.OfType<string>().FirstOrDefault(k => k == name);
        if (key != null && shopImportSettings.Services[key] is TService service)
            return service;

        return shopImportSettings.Services.Values.OfType<TService>().FirstOrDefault(v => v.ServiceTypeName == name);
    }

    public static IServiceSettings? GetService(this IShopImportSettings shopImportSettings, string name)    
    {
        return shopImportSettings.GetService<IServiceSettings>(name);
    }

    public static TService? GetImportService<TService>(this IShopImportSettings shopImportSettings)
        where TService : class, IServiceSettings
    {
        return shopImportSettings.GetService<TService>(nameof(PrimaryServiceName.ImportService));
    }   

    public static IServiceSettings? GetImportService(this IShopImportSettings shopImportSettings)      
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.ImportService));
    }   

    public static TService? GetBrowserDataLoader<TService>(this IShopImportSettings shopImportSettings)
        where TService : class, IServiceSettings
    {
        return shopImportSettings.GetService<TService>(nameof(PrimaryServiceName.BrowserDataLoader));
    }

    public static IServiceSettings? GetBrowserDataLoader(this IShopImportSettings shopImportSettings)
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.BrowserDataLoader));
    }

    public static TService? GetBrowserLauncher<TService>(this IShopImportSettings shopImportSettings)
        where TService : class, IServiceSettings
    {
        return shopImportSettings.GetService<TService>(nameof(PrimaryServiceName.BrowserLauncher));
    }

    public static IServiceSettings? GetBrowserLauncher(this IShopImportSettings shopImportSettings)    
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.BrowserLauncher));
    }

    public static TService? GetRequestHeaders<TService>(this IShopImportSettings shopImportSettings)
        where TService : class, IServiceSettings
    {
        return shopImportSettings.GetService<TService>(nameof(PrimaryServiceName.RequestHeaders));
    }

    public static IServiceSettings? GetRequestHeaders(this IShopImportSettings shopImportSettings)
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.RequestHeaders));
    }

    public static TService? GetWebLoader<TService>(this IShopImportSettings shopImportSettings)
        where TService : class, IServiceSettings
    {
        return shopImportSettings.GetService<TService>(nameof(PrimaryServiceName.WebLoader));
    }

    public static IServiceSettings? GetWebLoader(this IShopImportSettings shopImportSettings)    
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.WebLoader));
    }

}
