using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobExecutors;

internal interface IJobExecutor
{
    Task<string> Enqueue<T>(Expression<Func<T, Task>> jobTask, string waitingQueue, 
        ServiceExecuteOptions? serviceExecuteOptions = null, 
        CancellationToken cancellationToken = default);

    Task Execute<T>(string jobId, string processingQueue, CancellationToken cancellationToken = default);
}