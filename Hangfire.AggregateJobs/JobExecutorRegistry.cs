using Hangfire.AggregateJobs.JobExecutors;

namespace Hangfire.AggregateJobs;

internal class JobExecutorRegistry(IEnumerable<IJobExecutor> jobExecutors, IBackgroundJobClient backgroundJobClient) : IJobExecutorRegistry
{
    private readonly IEnumerable<IJobExecutor> _jobExecutors = jobExecutors;

    private readonly BackgroundJobExecutor DefaultJobExecutor = new(backgroundJobClient);

    public IJobExecutor Get(bool isChild = false, JobExecuteOptions? jobExecuteOptions = null, params object?[] parameters)
    {
        return _jobExecutors.FirstOrDefault(e => e.IsAccessible(isChild, jobExecuteOptions)) ?? DefaultJobExecutor;
    }
}