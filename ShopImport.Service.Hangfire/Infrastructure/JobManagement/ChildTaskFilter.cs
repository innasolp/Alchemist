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
        var jobStorage = scope.ServiceProvider.GetRequiredService<IJobStorage>();

        var parentJobId = jobStorage.GetParentJobId(jobId);

        if (string.IsNullOrEmpty(parentJobId)) return;

        jobStorage.UpdateJobState(jobId, 2);

        // todo
        RecurringJob.TriggerJob("child-orchestrator-tick");
    }

    public void OnPerforming(PerformingContext filterContext) { }
}