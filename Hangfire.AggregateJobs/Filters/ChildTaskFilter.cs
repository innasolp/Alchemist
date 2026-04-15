using Hangfire.AggregateJobs.ChildJobStorages;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.AggregateJobs.Filters;

internal class ChildTaskFilter(IServiceScopeFactory scopeFactory) : IServerFilter
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public void OnPerformed(PerformedContext filterContext)
    {
        var jobId = filterContext.BackgroundJob.Id;

        using var scope = _scopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

        var newJobState = filterContext.CancellationToken.ShutdownToken.IsCancellationRequested ? JobStatus.Deleted
             : filterContext.Exception != null ? JobStatus.Failed : JobStatus.Completed;

        childJobStorage.UpdateJobEntryState(jobId, newJobState, DateTime.Now);
    }

    public void OnPerforming(PerformingContext filterContext)
    {
        var jobId = filterContext.BackgroundJob.Id;

        using var scope = _scopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IAggregateJobStorage>();

        childJobStorage.UpdateJobEntryState(jobId, JobStatus.Processing, DateTime.Now);
    }
 }