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

    public static TService? GetService<TService>(this IImportSettings importSettings, string name)
        where TService : class, IServiceSettings
    {
        var key = importSettings.Services.Keys.OfType<string>().FirstOrDefault(k => k == name);
        if (key != null && importSettings.Services[key] is TService service)
            return service;

        return importSettings.Services.Values.OfType<TService>().FirstOrDefault(v => v.ServiceTypeName == name);
    }

    public static TService GetRequiredService<TService>(this IImportSettings importSettings, string name)
        where TService : class, IServiceSettings
    {
        var key = importSettings.Services.Keys.OfType<string>().FirstOrDefault(k => k == name);
        if (key != null && importSettings.Services[key] is TService service)
            return service;

        service = importSettings.Services.Values.OfType<TService>().FirstOrDefault(v => v.ServiceTypeName == name);

        return service ??
            throw new InvalidOperationException($"Service with name {name} not found.");
    }

    public static IServiceSettings? GetService(this IImportSettings importSettings, string name)    
    {
        return importSettings.GetService<IServiceSettings>(name);
    }

    public static IServiceSettings GetRequiredService(this IImportSettings importSettings, string name)    
    {
        return importSettings.GetRequiredService<IServiceSettings>(name);
    }

    public static TService? GetImportService<TService>(this IImportSettings importSettings)
        where TService : class, IServiceSettings
    {
        return importSettings.GetService<TService>(nameof(PrimaryServiceName.ImportService));
    }   

    public static IServiceSettings? GetImportService(this IImportSettings importSettings)      
    {
        return importSettings.GetService(nameof(PrimaryServiceName.ImportService));
    }   

    public static TService? GetBrowserDataLoader<TService>(this IImportSettings importSettings)
        where TService : class, IServiceSettings
    {
        return importSettings.GetService<TService>(nameof(PrimaryServiceName.BrowserDataLoader));
    }

    public static IServiceSettings? GetBrowserDataLoader(this IImportSettings importSettings)
    {
        return importSettings.GetService(nameof(PrimaryServiceName.BrowserDataLoader));
    }

    public static TService? GetBrowserLauncher<TService>(this IImportSettings importSettings)
        where TService : class, IServiceSettings
    {
        return importSettings.GetService<TService>(nameof(PrimaryServiceName.BrowserLauncher));
    }

    public static IServiceSettings? GetBrowserLauncher(this IImportSettings importSettings)    
    {
        return importSettings.GetService(nameof(PrimaryServiceName.BrowserLauncher));
    }

    public static TService? GetRequestHeaders<TService>(this IImportSettings importSettings)
        where TService : class, IServiceSettings
    {
        return importSettings.GetService<TService>(nameof(PrimaryServiceName.RequestHeaders));
    }

    public static IServiceSettings? GetRequestHeaders(this IImportSettings importSettings)
    {
        return importSettings.GetService(nameof(PrimaryServiceName.RequestHeaders));
    }

    public static TService? GetWebLoader<TService>(this IImportSettings importSettings)
        where TService : class, IServiceSettings
    {
        return importSettings.GetService<TService>(nameof(PrimaryServiceName.WebLoader));
    }

    public static IServiceSettings? GetWebLoader(this IImportSettings importSettings)    
    {
        return importSettings.GetService(nameof(PrimaryServiceName.WebLoader));
    }
    

    public static bool TryGetServiceStringValue(this IImportSettings importSettings, string serviceName, out string? value)
    {
        value = default;
        if (!importSettings.Services.Contains(serviceName) || importSettings.GetService(serviceName) is not IJsonNodeValue jsonValue
            || jsonValue?.ValueObj?.ValueKind != JsonValueKind.String
            || jsonValue.Value?.AsObject().ContainsKey("Value") != true)
            return false;

        value = jsonValue.Value["Value"]?.GetValue<string>();
        return true;
    }

    public static T? GetServiceValue<T>(this IServiceSettings service)
    {
        if(!string.IsNullOrEmpty(service.Value))
            return JsonSerializer.Deserialize<T>(service.Value);

        var filePath = !string.IsNullOrEmpty(service.AssemblyPath) 
            ? service.AssemblyPath 
            : service.ServiceProviderPath;
        
        if(!string.IsNullOrEmpty(filePath))
        {
            var text = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(text);
        }

        return default;
    }
}