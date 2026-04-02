using Hangfire.AggregateJobs.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.AggregateJobs;

internal class IdleJobChecker(IServiceScopeFactory serviceScopeFactory, IBackgroundJobClient backgroundJobClient)
{
    public const string Task = "idle-checker";

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

    [DisableConcurrentExecution(timeoutInSeconds: 30)]
    [ShortExpiration(minutes: 10)]
    [JobDisplayName(nameof(IdleJobChecker))]
    public async Task Dispatch(IJobCancellationToken? jobCancellationToken = null)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var childJobStorage = scope.ServiceProvider.GetRequiredService<IChildJobStorage>();

        var idleJobIds = await childJobStorage.GetIdleParentJobsIds(DateTime.Now, jobCancellationToken?.ShutdownToken ?? default);

        foreach (var idleJobId in idleJobIds)
        {
            _backgroundJobClient.Delete(idleJobId);

            childJobStorage.UpdateChildJobState(idleJobId, JobStatus.Deleted);
        }
    }
}