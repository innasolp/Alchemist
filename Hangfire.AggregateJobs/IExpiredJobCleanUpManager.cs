using Hangfire.AggregateJobs.Filters;
using Hangfire.Server;

namespace Hangfire.AggregateJobs;

internal interface IExpiredJobCleanUpManager
{
    [DisableConcurrentExecution(timeoutInSeconds: 30)]
    [ShortExpiration(minutes: 10)]
    [JobDisplayName("CleanUp")]
    Task Dispatch(IJobCancellationToken? jobCancellationToken = null, PerformContext? performContext = null);
}