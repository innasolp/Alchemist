using Hangfire;
using Hangfire.Server;
namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal class ChildTaskFilter(IJobStorage jobStorage) : IServerFilter
{
    private readonly IJobStorage _jobStorage = jobStorage;

    public void OnPerformed(PerformedContext filterContext)
    {
        var jobId = filterContext.BackgroundJob.Id;
        var parentJobId = _jobStorage.GetParentJobId(jobId);
        
        if (string.IsNullOrEmpty(parentJobId)) return;

        _jobStorage.UpdateJobState(jobId, 2);            

         // todo
         RecurringJob.TriggerJob("child-orchestrator-tick");        
    }

    public void OnPerforming(PerformingContext filterContext) { }
}