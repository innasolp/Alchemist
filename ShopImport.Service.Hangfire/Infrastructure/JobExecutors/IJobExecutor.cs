namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal interface IJobExecutor
{
    Task<string> Enqueue<T>(Func<T, Task> jobTask, string waitingQueue, CancellationToken cancellationToken = default);

    Task Execute(string jobId, string processingQueue, ServiceExecuteOptions? serviceExecuteOptions = null, CancellationToken cancellationToken = default);
}