using Hangfire.AggregateJobs.ChildJobStorages;
using Hangfire.AggregateJobs.JobExecutors;
using Hangfire.AggregateJobs.JobExecutors.Filter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.AggregateJobs;

public static class DIExtensions
{
    public static IServiceCollection AddHangfireAggreateJobs<T>(this IServiceCollection services, 
        string hangfireConnectionString,
        AggregateServerSettings aggregateServerSettings,
        Action<DbContextOptionsBuilder> childStorageOptionsAction,
        Action<IGlobalConfiguration, string> configureStorage,
        Action<IGlobalConfiguration>? configure = null)
    {
        services.AddHangfire((sp, config) =>
        {
            configureStorage?.Invoke(config, hangfireConnectionString);

            config.UseFilter(new ChildTaskFilter(sp.GetRequiredService<IServiceScopeFactory>()));
            config.UseFilter(new ChangeQueueFilter());

            configure?.Invoke(config);
        });

        services.AddDbContext<ChildJobDbContext>(childStorageOptionsAction);

        services.AddScoped<IChildJobStorage, EFChildJobStorage>();

        services.AddSingleton<ChildJobOrchestrator<T>>();

        services.AddScoped<IJobExecuteManager, JobExecuteManager>();

        services.AddScoped<IJobExecutor, BackgroundJobExecutor>();
        services.AddScoped<IJobExecutor, RecurringJobExecutor>();
        services.AddScoped<IJobExecutor, ScheduledJobExecutor>();       

        services.AddHangfireServer(options =>
        {
            options.Queues = [aggregateServerSettings.ProcessingQueue, "default"];
            options.WorkerCount = aggregateServerSettings.ParentWorkerCount;
            options.ServerName = aggregateServerSettings.ServerName;
        });

        services.AddHangfireServer(options =>
        {
            options.Queues = [aggregateServerSettings.ChildProcessingQueue];
            options.WorkerCount = aggregateServerSettings.ParentWorkerCount * aggregateServerSettings.ChildJobCountPerParent;
            options.ServerName = aggregateServerSettings.ChildServerName;
        });

        return services;
    }
}