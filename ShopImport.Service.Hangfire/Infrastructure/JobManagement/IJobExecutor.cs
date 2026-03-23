using System.Linq.Expressions;

namespace ShopImport.Service.Hangfire.Infrastructure.JobManagement;

internal interface IJobExecutor
{
    Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> jobTask, string waitingQueue, 
        ServiceExecuteOptions? serviceExecuteOptions = null, 
        CancellationToken cancellationToken = default);

    Task<string> ExecuteAsync<T>(string jobId, string processingQueue, CancellationToken cancellationToken = default);

    string Execute<T>(string jobId, string processingQueue);

    bool IsAccessible(bool isChild = false, ServiceExecuteOptions? serviceExecuteOptions = null);
}