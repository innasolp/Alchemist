using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.AggregateJobs;

internal class ChildTaskFilter(IServiceScopeFactory scopeFactory) : IServerFilter
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public void OnPerformed(PerformedContext filterContext)
    {
        var jobId = filterContext.BackgroundJob.Id;

        using var scope = _scopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IChildJobStorage>();

        var parentJobId = childJobStorage.GetParentJobId(jobId);

        if (string.IsNullOrEmpty(parentJobId) && childJobStorage.ParentJobExists(jobId))
        {
            childJobStorage.UpdateParentJobState(jobId,
                filterContext.CancellationToken.ShutdownToken.IsCancellationRequested ? JobStatus.Deleted : JobStatus.Completed);
            return;
        }

        childJobStorage.UpdateChildJobState(jobId,
        filterContext.CancellationToken.ShutdownToken.IsCancellationRequested ? JobStatus.Deleted : JobStatus.Completed);
    }

    public void OnPerforming(PerformingContext filterContext) { }
 }