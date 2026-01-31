using Microsoft.Extensions.DependencyInjection;

namespace BackgroundTaskQueue;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBoundedBackgroundQueue(this IServiceCollection services, int capacity)
    {
        return services.AddSingleton<IBackgroundTaskQueue>(new BoundedBackgroundTaskQueue(capacity));
    }

    public static IServiceCollection AddUnboundedBackgroundQueue(this IServiceCollection services, bool singleReader = false, bool singleWriter = false)
    {
        return services.AddSingleton<IBackgroundTaskQueue>(new UnboundedBackgroundTaskQueue(singleReader, singleWriter));
    }

    public static IServiceCollection AddKeyedBoundedBackgroundQueue(this IServiceCollection services, int capacity, object? key)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), $"value can't be zero or negative");

        return services.AddKeyedSingleton<IBackgroundTaskQueue>(key, new BoundedBackgroundTaskQueue(capacity));
    }

    public static IServiceCollection AddKeyedUnboundedBackgroundQueue(this IServiceCollection services, object? key, bool singleReader = false, bool singleWriter = false)
    {
        return services.AddKeyedSingleton<IBackgroundTaskQueue>(key, new UnboundedBackgroundTaskQueue(singleReader, singleWriter));
    }
}