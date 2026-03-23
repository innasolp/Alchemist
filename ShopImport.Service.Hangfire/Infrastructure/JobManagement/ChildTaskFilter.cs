using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.DependencyInjection;
namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal class ChildTaskFilter(IServiceScopeFactory scopeFactory) : IServerFilter
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public void OnPerformed(PerformedContext filterContext)
    {
        var jobId = filterContext.BackgroundJob.Id;

        using var scope = _scopeFactory.CreateScope();
        var childJobStorage = scope.ServiceProvider.GetRequiredService<IChildJobStorage>();

        var parentJobId = childJobStorage.GetParentJobId(jobId);

        if (string.IsNullOrEmpty(parentJobId)) return;

        childJobStorage.UpdateJobState(jobId, JobStatus.Completed);

        // todo
        RecurringJob.TriggerJob(ChildJobOrchestrator.Task);
    }

    public void OnPerforming(PerformingContext filterContext) { }
}