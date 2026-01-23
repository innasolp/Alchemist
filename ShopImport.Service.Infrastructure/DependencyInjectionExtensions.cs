using DependencyInjection.AssemblyExtensions;
using Import.Factory.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ShopImport.Category.Loader.Interfaces;

namespace ShopImport.Service.Category.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddImportCategoryInfrastructure(this IServiceCollection services, 
        string servicePath,
        string loadersPath)
    {
        services.AddServiceImplementationsFromPath(typeof(IImportServiceFactory), servicePath);
        return services.AddServiceImplementationsFromPath(typeof(ICategoryLoaderFactory), loadersPath);        
    }
}