using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.AggregateJobs.Filters;

internal static class FilterHelper
{
    public static IServiceScope CreateSafeScope(PerformContext performContext)
    {
        var activator = JobActivator.Current
                ?? throw new InvalidOperationException("Hangfire JobActivator is not initialized.");

        using var activatorScope = activator.BeginScope(performContext);

        var serviceProvider = activatorScope.Resolve(typeof(IServiceProvider)) as IServiceProvider
            ?? throw new InvalidOperationException("ServiceProvider not found in Hangfire Activator Scope.");

        return serviceProvider.CreateScope();
    }
}