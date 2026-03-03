using Alchemist.Import.Settings.Extensions;
using Import.Settings.Interfaces;
using ShopImport.KeyHash;
using ShopImport.ServiceState;

namespace ShopImport.Factory.Product.Json.Stateful;

internal static class ServiceStateExtensions
{
    private const string DefaultKeyHasherTypeName = "XxHash64";
    private const string DefaultServiceStateRepositoryProvider = "InMemory";

    public static IKeyHasher GetKeyHasher(this IEnumerable<IKeyHasher> keyHashers, IImportSettings importSettings )
    {
        if (!importSettings.TryGetServiceStringValue("KeyHasher", out var keyHasherTypeName) || string.IsNullOrEmpty(keyHasherTypeName))
            keyHasherTypeName = DefaultKeyHasherTypeName;

        var keyHasher = keyHashers.FirstOrDefault(k => k.GetType().ToString().Contains(keyHasherTypeName, StringComparison.CurrentCultureIgnoreCase));
        return keyHasher ??
            throw new InvalidDataException($"Keyhasher of type {keyHasherTypeName} not found.");
    }

    public static IServiceStateRepository GetServiceStateRepository(this IEnumerable<IServiceStateRepositoryFactory> factories, IImportSettings importSettings)
    {
        var serviceStateService = importSettings.GetService<IServiceSettings>("ServiceState");
        var serviceStateSettings = serviceStateService?.GetServiceValue<ServiceStateSettings>();

        var serviceStateRepositoryProvider = serviceStateSettings?.Provider ?? DefaultServiceStateRepositoryProvider;

        var serviceStateRepositoryFactory = factories.FirstOrDefault(f => f.GetType().Name.ToString().Contains(serviceStateRepositoryProvider, StringComparison.CurrentCultureIgnoreCase))
         ?? throw new InvalidDataException($"ServiceState repository provider {serviceStateRepositoryProvider} not found.");

        return !string.IsNullOrEmpty(serviceStateSettings?.ConnectionString)
            ? serviceStateRepositoryFactory.Create(serviceStateSettings.ConnectionString)
            : serviceStateRepositoryFactory.Create();
    }
}