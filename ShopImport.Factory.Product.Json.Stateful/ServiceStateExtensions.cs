using Alchemist.Import.Settings.Extensions;
using Import.Settings.Interfaces;
using ShopImport.KeyHash;
using ShopImport.ServiceState;

namespace ShopImport.Factory.Product.Json.Stateful;

internal static class ServiceStateExtensions
{
    public static IKeyHasher GetKeyHasher(this IEnumerable<IKeyHasher> keyHashers, IImportSettings importSettings )
    {
        var keyHasherService = importSettings.GetService<IServiceSettings>("KeyHasher");
        var keyHasherTypeName = keyHasherService?.GetServiceValue<string>();
        
        var keyHasher = keyHashers.FirstOrDefault(k => k.GetType().ToString().Equals(keyHasherTypeName, StringComparison.CurrentCultureIgnoreCase));
        return keyHasher ??
            throw new InvalidDataException($"Keyhasher of type {keyHasherTypeName} not found.");
    }

    public static IServiceStateRepository GetServiceStateRepository(this IEnumerable<IServiceStateRepositoryFactory> factories, IImportSettings importSettings)
    {
        var serviceStateService = importSettings.GetService<IServiceSettings>("ServiceState");
        var serviceStateSettings = serviceStateService?.GetServiceValue<ServiceStateSettings>();

        if (serviceStateSettings == null)
            return factories.First().Create();

        var serviceStateRepositoryFactory = factories.FirstOrDefault(f => f.GetType().Name.ToString().Contains(serviceStateSettings.Provider, StringComparison.CurrentCultureIgnoreCase));
        if(serviceStateRepositoryFactory == null)
            return factories.First().Create();

        return !string.IsNullOrEmpty(serviceStateSettings.ConnectionString)
            ? serviceStateRepositoryFactory.Create(serviceStateSettings.ConnectionString)
            : serviceStateRepositoryFactory.Create();
    }
}