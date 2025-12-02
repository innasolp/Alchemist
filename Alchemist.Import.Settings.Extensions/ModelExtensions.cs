using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Extensions;

public static class ModelExtensions
{    
    public static void Update<TService>(this TService serviceSettings, TService source)
        where TService : class, IServiceSettings
    {
        if (source == null) return;
        
        serviceSettings.ServiceProviderPath = source.ServiceProviderPath;
        serviceSettings.AssemblyPath = source.AssemblyPath;
        serviceSettings.ServiceTypeName = source.ServiceTypeName;
        serviceSettings.ImplementationTypeName = source.ImplementationTypeName;
        serviceSettings.Value = source.Value;
    }    

    public static void UpdateServices<TService>(this IShopImportSettings shopImportSettings, string serviceName, TService serviceSource)
        where TService : class, IServiceSettings 
    {
        var targetService = shopImportSettings.GetService<TService>(serviceName);

        if (targetService != null)
            targetService.Update(serviceSource);
        else
            shopImportSettings.Services.Add(serviceName, serviceSource);
    }

}
