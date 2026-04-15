namespace Hangfire.AggregateJobs.JobExecutors;

public interface IJobExecutorRegistry
{
    IJobExecutor Get(bool isChild = false, JobExecuteOptions? jobExecuteOptions = null, params object?[] parameters);
}