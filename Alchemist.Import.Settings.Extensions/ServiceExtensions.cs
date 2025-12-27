using Import.Settings.Interfaces;
using System.Text.Json;

namespace Alchemist.Import.Settings.Extensions;

public static class ServiceExtensions
{   
    public static bool IsPrimaryServiceName(this string serviceName)
    {
        var primaryServiceNames = Common.GetPrimaryServiceNames();
        return primaryServiceNames.Any(n => serviceName == n);
    }

    public static TService? GetService<TService>(this IImportSettings shopImportSettings, string name)
        where TService : class, IServiceSettings
    {
        var key = shopImportSettings.Services.Keys.OfType<string>().FirstOrDefault(k => k == name);
        if (key != null && shopImportSettings.Services[key] is TService service)
            return service;

        return shopImportSettings.Services.Values.OfType<TService>().FirstOrDefault(v => v.ServiceTypeName == name);
    }

    public static IServiceSettings? GetService(this IImportSettings shopImportSettings, string name)    
    {
        return shopImportSettings.GetService<IServiceSettings>(name);
    }

    public static TService? GetImportService<TService>(this IImportSettings shopImportSettings)
        where TService : class, IServiceSettings
    {
        return shopImportSettings.GetService<TService>(nameof(PrimaryServiceName.ImportService));
    }   

    public static IServiceSettings? GetImportService(this IImportSettings shopImportSettings)      
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.ImportService));
    }   

    public static TService? GetBrowserDataLoader<TService>(this IImportSettings shopImportSettings)
        where TService : class, IServiceSettings
    {
        return shopImportSettings.GetService<TService>(nameof(PrimaryServiceName.BrowserDataLoader));
    }

    public static IServiceSettings? GetBrowserDataLoader(this IImportSettings shopImportSettings)
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.BrowserDataLoader));
    }

    public static TService? GetBrowserLauncher<TService>(this IImportSettings shopImportSettings)
        where TService : class, IServiceSettings
    {
        return shopImportSettings.GetService<TService>(nameof(PrimaryServiceName.BrowserLauncher));
    }

    public static IServiceSettings? GetBrowserLauncher(this IImportSettings shopImportSettings)    
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.BrowserLauncher));
    }

    public static TService? GetRequestHeaders<TService>(this IImportSettings shopImportSettings)
        where TService : class, IServiceSettings
    {
        return shopImportSettings.GetService<TService>(nameof(PrimaryServiceName.RequestHeaders));
    }

    public static IServiceSettings? GetRequestHeaders(this IImportSettings shopImportSettings)
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.RequestHeaders));
    }

    public static TService? GetWebLoader<TService>(this IImportSettings shopImportSettings)
        where TService : class, IServiceSettings
    {
        return shopImportSettings.GetService<TService>(nameof(PrimaryServiceName.WebLoader));
    }

    public static IServiceSettings? GetWebLoader(this IImportSettings shopImportSettings)    
    {
        return shopImportSettings.GetService(nameof(PrimaryServiceName.WebLoader));
    }

    public static bool TryGetServiceStringValue(this IImportSettings shopImportSettings, string serviceName, out string? value)
    {
        value = default;
        if (!shopImportSettings.Services.Contains(serviceName) || shopImportSettings.GetService(serviceName) is not IJsonValue jsonValue
            || jsonValue?.ValueObj?.ValueKind != System.Text.Json.JsonValueKind.String
            || jsonValue.Value?.ContainsKey("Value") != true)
            return false;

        value = jsonValue.Value["Value"].GetValue<string>();
        return true;
    }

    public static T GetServiceValue<T>(this IServiceSettings service)
    {
        var filePath = !string.IsNullOrEmpty(service.AssemblyPath) 
            ? service.AssemblyPath 
            : !string.IsNullOrEmpty(service.ServiceProviderPath) ? service.ServiceProviderPath : string.Empty;
        
        if(!string.IsNullOrEmpty(filePath))
        {
            var text = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(text);
        }

        return JsonSerializer.Deserialize<T>(service.Value);
    }
}