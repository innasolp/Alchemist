using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.DependencyInjection.Common;

public static class DIExtensions
{
    public static IServiceCollection CollectServicesToEnumerable<T>(this IServiceCollection services, object[] serviceKeys, object enumerableKey)
        where T:class
    {
        return services.AddKeyedSingleton<IEnumerable<T>>(enumerableKey, (serviceProvider, key) =>
        {
            var collection = new List<T>();
            serviceKeys.ToList().ForEach(serviceKey => collection.Add(serviceProvider.GetRequiredKeyedService<T>(serviceKey)));
            return collection;
        });
    }

    public static IServiceCollection CollectServicesToDictionary<T>(this IServiceCollection services, Dictionary<object, object> serviceKeys, object enumerableKey)
        where T:class
    {
        return services.AddKeyedSingleton<IDictionary<object, T>>(enumerableKey, (serviceProvider, key) =>
        {
            var collection = new Dictionary<object, T>();
            serviceKeys.ToList().ForEach(serviceKey => collection.Add(serviceKey.Value, serviceProvider.GetRequiredKeyedService<T>(serviceKey.Key)));
            return collection;
        });
    }
}
