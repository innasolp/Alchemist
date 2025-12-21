using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.BackgroundTaskQueue;

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
}