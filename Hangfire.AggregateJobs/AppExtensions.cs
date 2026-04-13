using Hangfire.AggregateJobs.ChildJobStorages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.Common;

namespace Hangfire.AggregateJobs;

public static class AppExtensions
{
    public static void ClearChildJobStorage(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var childJobDbContext = scope.ServiceProvider.GetRequiredService<AggregateJobDbContext>();

        try
        {
            childJobDbContext.Database.EnsureDeleted();
        }
        catch (DbException ex) when (ex.SqlState == "3D000"){}

        childJobDbContext.Database.EnsureCreated();
    }

    public static void UseChildJobOrchestrator<T>(this IHost host, AggregateServerSettings aggregateServerSettings)
    {
        var recurringJobManager = host.Services.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<ChildJobOrchestrator<T>>(ChildJobOrchestrator.Task,
            x => x.Dispatch(aggregateServerSettings.ChildJobCountPerParent,
                            aggregateServerSettings.ChildServerName,
                            aggregateServerSettings.ChildProcessingQueue),
            Cron.Minutely());

        recurringJobManager.AddOrUpdate<IdleJobChecker>(IdleJobChecker.Task, x => x.Dispatch(null), Cron.Minutely());


        //todo
        using var scope = host.Services.CreateScope();
        var childJobDbContext = scope.ServiceProvider.GetRequiredService<AggregateJobDbContext>();
        childJobDbContext.Database.EnsureCreated();
    }
}