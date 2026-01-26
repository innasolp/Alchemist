using Microsoft.Extensions.DependencyInjection;

namespace BackgroundTaskQueue;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBoundedBackgroundQueue(this IServiceCollection services, int capacity)
    {
        return services.AddSingleton<IBackgroundTaskQueue>(new DefaultBoundedBackgroundTaskQueue(capacity));
    }

    public static IServiceCollection AddUnboundedBackgroundQueue(this IServiceCollection services, bool singleReader = false, bool singleWriter = false)
    {
        return services.AddSingleton<IBackgroundTaskQueue>(new DefaultUnboundedBackgroundTaskQueue(singleReader, singleWriter));
    }

    public static IServiceCollection AddKeyedBoundedBackgroundQueue(this IServiceCollection services, int capacity, object? key)
    {
        return services.AddKeyedSingleton<IBackgroundTaskQueue>(key, new DefaultBoundedBackgroundTaskQueue(capacity));
    }
}